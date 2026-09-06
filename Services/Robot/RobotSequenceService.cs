using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Services.Robot;

public class RobotSequenceService : IRobotSequenceService
{
    private const int MaxSequenceLength = 100;

    private readonly RobotContext _context;
    private readonly IRobotMovementService _robotMovementService;
    private readonly IRobotSequenceQueue _sequenceQueue;
    private readonly IRobotUpdateNotifier _robotUpdateNotifier;
    private readonly ILogger<RobotSequenceService> _logger;

    public RobotSequenceService(
        RobotContext context,
        IRobotMovementService robotMovementService,
        IRobotSequenceQueue sequenceQueue,
        IRobotUpdateNotifier robotUpdateNotifier,
        ILogger<RobotSequenceService> logger)
    {
        _context = context;
        _robotMovementService = robotMovementService;
        _sequenceQueue = sequenceQueue;
        _robotUpdateNotifier = robotUpdateNotifier;
        _logger = logger;
    }

    public async Task<RobotSequenceSubmissionResult> SubmitAsync(int userId, RobotSequenceSubmitRequest request, CancellationToken cancellationToken)
    {
        if (request.CommandIds.Count == 0)
        {
            throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid movement sequence", "At least one movement command is required.");
        }

        if (request.CommandIds.Count > MaxSequenceLength)
        {
            throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid movement sequence", $"A movement sequence can contain at most {MaxSequenceLength} commands.");
        }

        var activeExists = await _context.RobotCommandSequences.AnyAsync(
            s => s.UserId == userId && (s.Status == RobotSequenceStatus.Queued || s.Status == RobotSequenceStatus.Executing),
            cancellationToken);

        if (activeExists)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "Robot is busy", "Another movement sequence is currently queued or executing.");
        }

        var state = await _context.RobotStates.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (state == null || state.MapId == null)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "No map selected", "Select a map before placing or moving the robot.");
        }

        if (!state.X.HasValue || !state.Y.HasValue)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "Robot has not been placed", "Place the robot on the selected map before executing movements.");
        }

        var map = await _context.Maps.AsNoTracking().FirstOrDefaultAsync(m => m.Id == state.MapId.Value, cancellationToken);
        if (map == null)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "Selected map not found", "The selected map no longer exists.");
        }

        var distinctIds = request.CommandIds.Distinct().ToList();
        var commands = await _context.RobotCommands
            .Where(command => distinctIds.Contains(command.Id))
            .ToListAsync(cancellationToken);

        var commandLookup = commands.ToDictionary(command => command.Id);
        var orderedCommands = new List<RobotCommand>(request.CommandIds.Count);

        foreach (var commandId in request.CommandIds)
        {
            if (!commandLookup.TryGetValue(commandId, out var command))
            {
                throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid movement sequence", $"Command {commandId} does not exist.");
            }

            if (!command.IsMoveCommand)
            {
                throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid movement sequence", $"Command {command.Name} is not a movement command.");
            }

            if (command.MovementDirection == null)
            {
                throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid movement sequence", $"Command {command.Name} has no movement direction.");
            }

            orderedCommands.Add(command);
        }

        var start = new RobotPosition(state.X.Value, state.Y.Value);
        var validation = _robotMovementService.ValidateSequence(map, start, orderedCommands, request.TargetX, request.TargetY);
        if (!validation.IsValid)
        {
            var exception = new RobotDomainException(
                StatusCodes.Status400BadRequest,
                validation.ErrorTitle ?? "Invalid movement sequence",
                validation.ErrorDetail ?? "The movement sequence is invalid.");

            if (validation.FailingStep.HasValue)
            {
                exception.WithExtension("step", validation.FailingStep.Value);
            }

            if (validation.FailingPosition.HasValue)
            {
                exception.WithExtension("position", new { x = validation.FailingPosition.Value.X, y = validation.FailingPosition.Value.Y });
            }

            throw exception;
        }

        var now = DateTime.UtcNow;
        var sequence = new RobotCommandSequence
        {
            UserId = userId,
            MapId = map.Id,
            Status = RobotSequenceStatus.Queued,
            StartX = start.X,
            StartY = start.Y,
            FinalX = null,
            FinalY = null,
            CurrentStep = 0,
            TotalSteps = orderedCommands.Count,
            CancellationRequested = false,
            CreatedDate = now,
            ModifiedDate = now
        };

        _context.RobotCommandSequences.Add(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        var items = orderedCommands
            .Select((command, index) => new RobotCommandSequenceItem
            {
                SequenceId = sequence.Id,
                RobotCommandId = command.Id,
                Order = index + 1,
                IsExecuted = false,
                CommandName = command.Name,
                MovementDirection = command.MovementDirection!.Value
            })
            .ToList();

        _context.RobotCommandSequenceItems.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);

        await _sequenceQueue.QueueAsync(sequence.Id, cancellationToken);
        await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);

        _logger.LogInformation("Robot movement sequence {SequenceId} submitted by user {UserId} with {TotalSteps} steps", sequence.Id, userId, sequence.TotalSteps);

        return new RobotSequenceSubmissionResult
        {
            Sequence = sequence,
            PredictedFinalPosition = validation.FinalPosition
        };
    }

    public async Task<RobotCommandSequence?> GetByIdForUserAsync(int userId, int sequenceId, CancellationToken cancellationToken)
    {
        return await _context.RobotCommandSequences
            .AsNoTracking()
            .Include(sequence => sequence.Items.OrderBy(item => item.Order))
            .FirstOrDefaultAsync(sequence => sequence.Id == sequenceId && sequence.UserId == userId, cancellationToken);
    }

    public async Task<List<RobotCommandSequence>> ListForUserAsync(int userId, RobotSequenceStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.RobotCommandSequences
            .AsNoTracking()
            .Where(sequence => sequence.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(sequence => sequence.Status == status.Value);
        }

        return await query
            .OrderByDescending(sequence => sequence.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<RobotCommandSequence> RequestCancelAsync(int userId, int sequenceId, CancellationToken cancellationToken)
    {
        var sequence = await _context.RobotCommandSequences
            .FirstOrDefaultAsync(s => s.Id == sequenceId && s.UserId == userId, cancellationToken);

        if (sequence == null)
        {
            throw new RobotDomainException(StatusCodes.Status404NotFound, "Sequence not found", $"Sequence {sequenceId} was not found.");
        }

        var now = DateTime.UtcNow;

        switch (sequence.Status)
        {
            case RobotSequenceStatus.Queued:
                sequence.Status = RobotSequenceStatus.Cancelled;
                sequence.CancellationRequested = true;
                sequence.CompletedDate = now;
                sequence.ModifiedDate = now;
                break;
            case RobotSequenceStatus.Executing:
                sequence.CancellationRequested = true;
                sequence.ModifiedDate = now;
                break;
            case RobotSequenceStatus.Completed:
            case RobotSequenceStatus.Failed:
            case RobotSequenceStatus.Cancelled:
                throw new RobotDomainException(StatusCodes.Status409Conflict, "Sequence cannot be cancelled", "The sequence cannot be cancelled in its current status.");
            default:
                throw new RobotDomainException(StatusCodes.Status409Conflict, "Sequence cannot be cancelled", "The sequence cannot be cancelled in its current status.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);

        _logger.LogInformation("Cancellation requested for sequence {SequenceId} by user {UserId}", sequenceId, userId);
        return sequence;
    }

    public async Task<RobotCommandSequence?> TryStartExecutionAsync(int sequenceId, CancellationToken cancellationToken)
    {
        var sequence = await _context.RobotCommandSequences
            .Include(s => s.Items.OrderBy(i => i.Order))
            .FirstOrDefaultAsync(s => s.Id == sequenceId, cancellationToken);

        if (sequence == null)
        {
            return null;
        }

        if (sequence.Status == RobotSequenceStatus.Cancelled || sequence.Status == RobotSequenceStatus.Completed || sequence.Status == RobotSequenceStatus.Failed)
        {
            return null;
        }

        if (sequence.CancellationRequested)
        {
            sequence.Status = RobotSequenceStatus.Cancelled;
            sequence.CompletedDate = DateTime.UtcNow;
            sequence.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);
            return null;
        }

        if (sequence.Status != RobotSequenceStatus.Queued)
        {
            return null;
        }

        sequence.Status = RobotSequenceStatus.Executing;
        sequence.StartedDate = DateTime.UtcNow;
        sequence.ModifiedDate = DateTime.UtcNow;
        sequence.FailureReason = null;

        await _context.SaveChangesAsync(cancellationToken);
        await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);
        return sequence;
    }

    public async Task MarkCompletedAsync(int sequenceId, int finalX, int finalY, CancellationToken cancellationToken)
    {
        var sequence = await _context.RobotCommandSequences.FirstOrDefaultAsync(s => s.Id == sequenceId, cancellationToken);
        if (sequence == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        sequence.Status = RobotSequenceStatus.Completed;
        sequence.FinalX = finalX;
        sequence.FinalY = finalY;
        sequence.CurrentStep = sequence.TotalSteps;
        sequence.CompletedDate = now;
        sequence.ModifiedDate = now;
        sequence.FailureReason = null;

        await _context.SaveChangesAsync(cancellationToken);
        await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);
    }

    public async Task MarkFailedAsync(int sequenceId, string failureReason, CancellationToken cancellationToken)
    {
        var sequence = await _context.RobotCommandSequences.FirstOrDefaultAsync(s => s.Id == sequenceId, cancellationToken);
        if (sequence == null || sequence.Status == RobotSequenceStatus.Cancelled)
        {
            return;
        }

        var now = DateTime.UtcNow;
        sequence.Status = RobotSequenceStatus.Failed;
        sequence.FailureReason = failureReason;
        sequence.CompletedDate = now;
        sequence.ModifiedDate = now;

        await _context.SaveChangesAsync(cancellationToken);
        await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);
    }

    private async Task NotifySequenceUpdateSafeAsync(RobotCommandSequence sequence, CancellationToken cancellationToken)
    {
        var update = new RobotSequenceUpdateDto(
            sequence.Id,
            sequence.Status.ToString(),
            sequence.CurrentStep,
            sequence.TotalSteps,
            DateTime.UtcNow,
            sequence.FailureReason);

        try
        {
            await _robotUpdateNotifier.NotifySequenceUpdatedAsync(update, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Realtime sequence update notification failed for sequence {SequenceId}", sequence.Id);
        }
    }
}
