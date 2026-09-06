using Microsoft.EntityFrameworkCore;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandService : IRobotCommandService
    {
        private readonly RobotContext _context;
        private readonly IRobotCommandQueue _queue;
        private readonly IRobotUpdateNotifier _robotUpdateNotifier;
        private readonly ILogger<RobotCommandService> _logger;

        public RobotCommandService(
            RobotContext context,
            IRobotCommandQueue queue,
            IRobotUpdateNotifier robotUpdateNotifier,
            ILogger<RobotCommandService> logger)
        {
            _context = context;
            _queue = queue;
            _robotUpdateNotifier = robotUpdateNotifier;
            _logger = logger;
        }

        public async Task<RobotCommand> SubmitAsync(RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("no command name provided");
            }

            var commandName = request.Name.Trim();

            if (request.IsMoveCommand && request.MovementDirection == null)
            {
                throw new ArgumentException("movement direction is required for move commands");
            }

            if (!request.IsMoveCommand && request.MovementDirection != null)
            {
                throw new ArgumentException("movement direction must be null for non-move commands");
            }

            var duplicateExists = await _context.RobotCommands
                .AnyAsync(command => command.Name == commandName, cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException("command already exists");
            }

            var now = DateTime.UtcNow;
            var command = new RobotCommand
            {
                Name = commandName,
                Description = request.Description,
                IsMoveCommand = request.IsMoveCommand,
                MovementDirection = request.MovementDirection,
                Status = RobotCommandStatus.Pending,
                CreatedDate = now,
                ModifiedDate = now
            };

            _context.RobotCommands.Add(command);
            await _context.SaveChangesAsync(cancellationToken);

            await _queue.QueueAsync(command.Id, cancellationToken);

            command.Status = RobotCommandStatus.Queued;
            command.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await NotifyCommandUpdatedSafeAsync(command, cancellationToken);

            _logger.LogInformation("Robot command {CommandId} queued for asynchronous execution", command.Id);
            return command;
        }

        public async Task<RobotCommandStatusDto?> GetStatusAsync(int id, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            return command == null ? null : ToStatusDto(command);
        }

        public async Task<RobotCommandCancellationResult> CancelAsync(int id, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (command == null)
            {
                return RobotCommandCancellationResult.NotFound;
            }

            if (command.Status != RobotCommandStatus.Pending && command.Status != RobotCommandStatus.Queued)
            {
                return RobotCommandCancellationResult.NotAllowed;
            }

            var now = DateTime.UtcNow;
            command.Status = RobotCommandStatus.Cancelled;
            command.ModifiedDate = now;
            command.CompletedDate = now;
            command.FailureReason = null;

            await _context.SaveChangesAsync(cancellationToken);
            await NotifyCommandUpdatedSafeAsync(command, cancellationToken);
            return RobotCommandCancellationResult.Cancelled;
        }

        public async Task<RobotCommand?> TryStartExecutionAsync(int id, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (command == null)
            {
                return null;
            }

            if (command.Status == RobotCommandStatus.Cancelled ||
                command.Status == RobotCommandStatus.Completed ||
                command.Status == RobotCommandStatus.Failed)
            {
                return null;
            }

            if (command.Status != RobotCommandStatus.Pending && command.Status != RobotCommandStatus.Queued)
            {
                return null;
            }

            command.Status = RobotCommandStatus.Executing;
            command.StartedDate = DateTime.UtcNow;
            command.ModifiedDate = DateTime.UtcNow;
            command.FailureReason = null;
            await _context.SaveChangesAsync(cancellationToken);

            await NotifyCommandUpdatedSafeAsync(command, cancellationToken);

            return command;
        }

        public async Task MarkCompletedAsync(int id, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (command == null || command.Status == RobotCommandStatus.Cancelled)
            {
                return;
            }

            var now = DateTime.UtcNow;
            command.Status = RobotCommandStatus.Completed;
            command.CompletedDate = now;
            command.ModifiedDate = now;
            command.FailureReason = null;

            await _context.SaveChangesAsync(cancellationToken);
            await NotifyCommandUpdatedSafeAsync(command, cancellationToken);
        }

        public async Task MarkFailedAsync(int id, string failureReason, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (command == null || command.Status == RobotCommandStatus.Cancelled)
            {
                return;
            }

            var now = DateTime.UtcNow;
            command.Status = RobotCommandStatus.Failed;
            command.FailureReason = failureReason;
            command.CompletedDate = now;
            command.ModifiedDate = now;

            await _context.SaveChangesAsync(cancellationToken);
            await NotifyCommandUpdatedSafeAsync(command, cancellationToken);
        }

        private async Task NotifyCommandUpdatedSafeAsync(RobotCommand command, CancellationToken cancellationToken)
        {
            var update = new RobotCommandUpdateDto(
                command.Id,
                command.Name,
                command.Status.ToString(),
                command.CreatedDate,
                command.StartedDate,
                command.CompletedDate,
                command.ModifiedDate,
                command.FailureReason);

            try
            {
                await _robotUpdateNotifier.NotifyCommandUpdatedAsync(update, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Realtime command update notification failed for command {CommandId}",
                    command.Id);
            }
        }

        private static RobotCommandStatusDto ToStatusDto(RobotCommand command)
        {
            return new RobotCommandStatusDto
            {
                Id = command.Id,
                Name = command.Name,
                Description = command.Description,
                IsMoveCommand = command.IsMoveCommand,
                MovementDirection = command.MovementDirection,
                Status = command.Status.ToString(),
                CreatedDate = command.CreatedDate,
                StartedDate = command.StartedDate,
                CompletedDate = command.CompletedDate,
                ModifiedDate = command.ModifiedDate,
                FailureReason = command.FailureReason
            };
        }
    }
}
