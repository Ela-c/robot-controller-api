using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public interface IRobotSequenceExecutor
{
    Task<RobotSequenceExecutionResult> ExecuteAsync(RobotCommandSequence sequence, CancellationToken cancellationToken);
}
