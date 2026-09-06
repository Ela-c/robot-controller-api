using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Models;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandExecutor : IRobotCommandExecutor
    {
        private readonly IRobotUpdateNotifier _robotUpdateNotifier;
        private readonly ILogger<RobotCommandExecutor> _logger;

        public RobotCommandExecutor(IRobotUpdateNotifier robotUpdateNotifier, ILogger<RobotCommandExecutor> logger)
        {
            _robotUpdateNotifier = robotUpdateNotifier;
            _logger = logger;
        }

        public async Task ExecuteAsync(RobotCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Name, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Simulated robot execution failure.");
            }

            if (!command.IsMoveCommand)
            {
                await Task.Delay(150, cancellationToken);
                return;
            }

            for (var step = 1; step <= 3; step++)
            {
                await Task.Delay(100, cancellationToken);
                await NotifyPositionSafeAsync(command.Id, step, 0, cancellationToken);
            }
        }

        private async Task NotifyPositionSafeAsync(int commandId, int x, int y, CancellationToken cancellationToken)
        {
            var update = new RobotPositionUpdateDto(commandId, x, y, DateTime.UtcNow);
            try
            {
                await _robotUpdateNotifier.NotifyPositionUpdatedAsync(update, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Realtime position update notification failed for command {CommandId}",
                    commandId);
            }
        }
    }
}
