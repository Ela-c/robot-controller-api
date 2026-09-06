using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public class RobotSequenceSubmissionResult
{
    public required RobotCommandSequence Sequence { get; init; }
    public required RobotPosition PredictedFinalPosition { get; init; }
}
