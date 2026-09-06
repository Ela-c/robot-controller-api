using Microsoft.EntityFrameworkCore;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandService : IRobotCommandService
    {
        private readonly RobotContext _context;
        private readonly ILogger<RobotCommandService> _logger;

        public RobotCommandService(
            RobotContext context,
            ILogger<RobotCommandService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RobotCommand> SubmitAsync(RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("no command name provided");
            }

            var commandName = request.Name.Trim();

            var movementDirections = request.MovementDirections?
                .Where(direction => Enum.IsDefined(typeof(MovementDirection), direction))
                .ToList() ?? new List<MovementDirection>();

            if (request.IsMoveCommand)
            {
                if (movementDirections.Count == 0 && request.MovementDirection != null)
                {
                    movementDirections.Add(request.MovementDirection.Value);
                }

                if (movementDirections.Count == 0)
                {
                    throw new ArgumentException("at least one movement direction is required for move commands");
                }
            }

            if (!request.IsMoveCommand && (request.MovementDirection != null || movementDirections.Count > 0))
            {
                throw new ArgumentException("movement directions must be null/empty for non-move commands");
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
                MovementDirection = request.IsMoveCommand ? movementDirections[0] : null,
                CreatedDate = now,
                ModifiedDate = now
            };

            _context.RobotCommands.Add(command);
            await _context.SaveChangesAsync(cancellationToken);

            if (request.IsMoveCommand)
            {
                var commandSteps = movementDirections
                    .Select((direction, index) => new RobotCommandStep
                    {
                        RobotCommandId = command.Id,
                        Order = index + 1,
                        MovementDirection = direction
                    })
                    .ToList();

                _context.RobotCommandSteps.AddRange(commandSteps);
                await _context.SaveChangesAsync(cancellationToken);
                command.Steps = commandSteps;
            }

            _logger.LogInformation("Robot command definition {CommandId} created", command.Id);
            return command;
        }

        public async Task<RobotCommandStatusDto?> GetStatusAsync(int id, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .AsNoTracking()
                .Include(c => c.Steps)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            return command == null ? null : ToStatusDto(command);
        }

        public async Task<RobotCommand?> UpdateAsync(int id, RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
        {
            var command = await _context.RobotCommands
                .Include(c => c.Steps)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (command == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("no command name provided");
            }

            var commandName = request.Name.Trim();
            var duplicateExists = await _context.RobotCommands
                .AnyAsync(existing => existing.Id != id && existing.Name == commandName, cancellationToken);
            if (duplicateExists)
            {
                throw new InvalidOperationException("command already exists");
            }

            var movementDirections = request.MovementDirections?
                .Where(direction => Enum.IsDefined(typeof(MovementDirection), direction))
                .ToList() ?? new List<MovementDirection>();

            if (request.IsMoveCommand)
            {
                if (movementDirections.Count == 0 && request.MovementDirection != null)
                {
                    movementDirections.Add(request.MovementDirection.Value);
                }

                if (movementDirections.Count == 0)
                {
                    throw new ArgumentException("at least one movement direction is required for move commands");
                }
            }

            if (!request.IsMoveCommand && (request.MovementDirection != null || movementDirections.Count > 0))
            {
                throw new ArgumentException("movement directions must be null/empty for non-move commands");
            }

            command.Name = commandName;
            command.Description = request.Description;
            command.IsMoveCommand = request.IsMoveCommand;
            command.MovementDirection = request.IsMoveCommand ? movementDirections[0] : null;
            command.ModifiedDate = DateTime.UtcNow;

            _context.RobotCommandSteps.RemoveRange(command.Steps);
            command.Steps.Clear();

            if (request.IsMoveCommand)
            {
                var steps = movementDirections.Select((direction, index) => new RobotCommandStep
                {
                    RobotCommandId = command.Id,
                    Order = index + 1,
                    MovementDirection = direction
                }).ToList();

                _context.RobotCommandSteps.AddRange(steps);
                command.Steps = steps;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return command;
        }

        public async Task<RobotCommandCancellationResult> CancelAsync(int id, CancellationToken cancellationToken)
        {
            var exists = await _context.RobotCommands.AnyAsync(c => c.Id == id, cancellationToken);
            return exists ? RobotCommandCancellationResult.NotAllowed : RobotCommandCancellationResult.NotFound;
        }

        public Task<RobotCommand?> TryStartExecutionAsync(int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<RobotCommand?>(null);
        }

        public Task MarkCompletedAsync(int id, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(int id, string failureReason, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static RobotCommandStatusDto ToStatusDto(RobotCommand command)
        {
            var movementDirections = command.Steps
                .OrderBy(step => step.Order)
                .Select(step => step.MovementDirection)
                .ToList();

            if (movementDirections.Count == 0 && command.IsMoveCommand && command.MovementDirection.HasValue)
            {
                movementDirections.Add(command.MovementDirection.Value);
            }

            return new RobotCommandStatusDto
            {
                Id = command.Id,
                Name = command.Name,
                Description = command.Description,
                IsMoveCommand = command.IsMoveCommand,
                MovementDirection = command.MovementDirection,
                MovementDirections = movementDirections,
                CreatedDate = command.CreatedDate,
                ModifiedDate = command.ModifiedDate
            };
        }
    }
}
