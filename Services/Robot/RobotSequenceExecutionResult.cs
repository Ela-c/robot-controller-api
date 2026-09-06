namespace robot_controller_api.Services.Robot;

public class RobotSequenceExecutionResult
{
    public required bool Cancelled { get; init; }
    public required RobotPosition FinalPosition { get; init; }
}
