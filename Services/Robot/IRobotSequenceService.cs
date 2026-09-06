using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public interface IRobotSequenceService
{
    Task<RobotSequenceSubmissionResult> SubmitAsync(int userId, RobotSequenceSubmitRequest request, CancellationToken cancellationToken);
    Task<RobotCommandSequence?> GetByIdForUserAsync(int userId, int sequenceId, CancellationToken cancellationToken);
    Task<List<RobotCommandSequence>> ListForUserAsync(int userId, RobotSequenceStatus? status, CancellationToken cancellationToken);
    Task<RobotCommandSequence> RequestCancelAsync(int userId, int sequenceId, CancellationToken cancellationToken);
    Task<RobotCommandSequence?> TryQueueExecutionAsync(int sequenceId, CancellationToken cancellationToken);
    Task<RobotCommandSequence?> TryStartExecutionAsync(int sequenceId, CancellationToken cancellationToken);
    Task MarkCompletedAsync(int sequenceId, int finalX, int finalY, CancellationToken cancellationToken);
    Task MarkFailedAsync(int sequenceId, string failureReason, CancellationToken cancellationToken);
}
