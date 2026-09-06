using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;

namespace robot_controller_api.Services.RobotCommands
{
    public interface IRobotCommandService
    {
        Task<RobotCommand> SubmitAsync(RobotCommandSubmitRequestDto request, CancellationToken cancellationToken);
        Task<RobotCommandStatusDto?> GetStatusAsync(int id, CancellationToken cancellationToken);
        Task<RobotCommandCancellationResult> CancelAsync(int id, CancellationToken cancellationToken);
        Task<RobotCommand?> TryStartExecutionAsync(int id, CancellationToken cancellationToken);
        Task MarkCompletedAsync(int id, CancellationToken cancellationToken);
        Task MarkFailedAsync(int id, string failureReason, CancellationToken cancellationToken);
    }
}
