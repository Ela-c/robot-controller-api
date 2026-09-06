using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public class SequenceValidationResult
{
    public bool IsValid { get; init; }
    public RobotPosition FinalPosition { get; init; }
    public int? FailingStep { get; init; }
    public RobotPosition? FailingPosition { get; init; }
    public RobotCommand? FailingCommand { get; init; }
    public string? ErrorTitle { get; init; }
    public string? ErrorDetail { get; init; }
}
