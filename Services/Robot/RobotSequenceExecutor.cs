using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Services.Robot;

public class RobotSequenceExecutor : IRobotSequenceExecutor
{
    private readonly RobotContext _context;
    private readonly IRobotMovementService _movementService;
    private readonly IRobotUpdateNotifier _robotUpdateNotifier;
    private readonly RobotExecutionOptions _options;
    private readonly ILogger<RobotSequenceExecutor> _logger;

    public RobotSequenceExecutor(
        RobotContext context,
        IRobotMovementService movementService,
        IRobotUpdateNotifier robotUpdateNotifier,
        IOptions<RobotExecutionOptions> options,
        ILogger<RobotSequenceExecutor> logger)
    {
        _context = context;
        _movementService = movementService;
        _robotUpdateNotifier = robotUpdateNotifier;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RobotSequenceExecutionResult> ExecuteAsync(RobotCommandSequence sequence, CancellationToken cancellationToken)
    {
        var map = await _context.Maps.FirstOrDefaultAsync(m => m.Id == sequence.MapId, cancellationToken);
        if (map == null)
        {
            throw new InvalidOperationException("Selected map no longer exists.");
        }

        var state = await _context.RobotStates.FirstOrDefaultAsync(s => s.UserId == sequence.UserId, cancellationToken);
        if (state == null || !state.X.HasValue || !state.Y.HasValue)
        {
            throw new InvalidOperationException("Robot has not been placed.");
        }

        var finalPosition = new RobotPosition(state.X.Value, state.Y.Value);

        var items = await _context.RobotCommandSequenceItems
            .Where(i => i.SequenceId == sequence.Id)
            .OrderBy(i => i.Order)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            await _context.Entry(sequence).ReloadAsync(cancellationToken);
            if (sequence.CancellationRequested)
            {
                var now = DateTime.UtcNow;
                sequence.Status = RobotSequenceStatus.Cancelled;
                sequence.FinalX = finalPosition.X;
                sequence.FinalY = finalPosition.Y;
                sequence.CompletedDate = now;
                sequence.ModifiedDate = now;
                await _context.SaveChangesAsync(cancellationToken);
                await NotifySequenceUpdateSafeAsync(sequence, cancellationToken);

                _logger.LogInformation("Sequence {SequenceId} cancelled after step {Step}", sequence.Id, sequence.CurrentStep);

                return new RobotSequenceExecutionResult
                {
                    Cancelled = true,
                    FinalPosition = finalPosition
                };
            }

            if (item.IsExecuted)
            {
                continue;
            }

            var next = _movementService.CalculateNextPosition(finalPosition, item.MovementDirection);
            if (!_movementService.IsWithinMap(map, next))
            {
                throw new InvalidOperationException($"Step {item.Order} ({item.CommandName}) would move the robot outside the map.");
            }

            var nowStep = DateTime.UtcNow;
            state.X = next.X;
            state.Y = next.Y;
            state.ModifiedDate = nowStep;

            item.IsExecuted = true;
            item.ExecutedDate = nowStep;

            sequence.CurrentStep = item.Order;
            sequence.ModifiedDate = nowStep;

            await _context.SaveChangesAsync(cancellationToken);
            await NotifySequencePositionSafeAsync(sequence, map.Id, next.X, next.Y, item.Order, cancellationToken);

            finalPosition = next;

            _logger.LogInformation("Sequence {SequenceId} executed step {Step}/{Total} new position ({X},{Y})", sequence.Id, item.Order, sequence.TotalSteps, next.X, next.Y);

            if (_options.StepDelayMs > 0)
            {
                await Task.Delay(_options.StepDelayMs, cancellationToken);
            }
        }

        return new RobotSequenceExecutionResult
        {
            Cancelled = false,
            FinalPosition = finalPosition
        };
    }

    private async Task NotifySequencePositionSafeAsync(RobotCommandSequence sequence, int mapId, int x, int y, int step, CancellationToken cancellationToken)
    {
        try
        {
            await _robotUpdateNotifier.NotifySequencePositionUpdatedAsync(
                new RobotSequencePositionUpdateDto(sequence.Id, mapId, x, y, step, sequence.TotalSteps, DateTime.UtcNow),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Realtime sequence position notification failed for sequence {SequenceId}", sequence.Id);
        }
    }

    private async Task NotifySequenceUpdateSafeAsync(RobotCommandSequence sequence, CancellationToken cancellationToken)
    {
        try
        {
            await _robotUpdateNotifier.NotifySequenceUpdatedAsync(
                new RobotSequenceUpdateDto(sequence.Id, sequence.Status.ToString(), sequence.CurrentStep, sequence.TotalSteps, DateTime.UtcNow, sequence.FailureReason),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Realtime sequence status notification failed for sequence {SequenceId}", sequence.Id);
        }
    }
}
