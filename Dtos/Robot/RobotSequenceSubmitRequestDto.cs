namespace robot_controller_api.Dtos.Robot;

public class RobotSequenceSubmitRequestDto
{
    public List<int> CommandIds { get; set; } = new();
    public int? TargetX { get; set; }
    public int? TargetY { get; set; }
}
