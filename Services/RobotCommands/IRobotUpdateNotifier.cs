using robot_controller_api.Dtos.Realtime;

namespace robot_controller_api.Services.RobotCommands;

public interface IRobotUpdateNotifier
{
    Task NotifyCommandUpdatedAsync(RobotCommandUpdateDto update, CancellationToken cancellationToken = default);
    Task NotifyPositionUpdatedAsync(RobotPositionUpdateDto update, CancellationToken cancellationToken = default);
}