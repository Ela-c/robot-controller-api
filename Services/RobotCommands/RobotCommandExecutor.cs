using robot_controller_api.Models;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandExecutor : IRobotCommandExecutor
    {
        public async Task ExecuteAsync(RobotCommand command, CancellationToken cancellationToken)
        {
            if (string.Equals(command.Name, "FAIL", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Simulated robot execution failure.");
            }

            var delay = command.IsMoveCommand ? 300 : 150;
            await Task.Delay(delay, cancellationToken);
        }
    }
}
