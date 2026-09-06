using robot_controller_api.Models;

namespace robot_controller_api.Services.RobotCommands
{
    public interface IRobotCommandExecutor
    {
        Task ExecuteAsync(RobotCommand command, CancellationToken cancellationToken);
    }
}
