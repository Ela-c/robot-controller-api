namespace robot_controller_api.Services.Robot;

public class RobotSequenceSubmitRequest
{
    public IReadOnlyList<int> CommandIds { get; init; } = Array.Empty<int>();
    public int? TargetX { get; init; }
    public int? TargetY { get; init; }
}
