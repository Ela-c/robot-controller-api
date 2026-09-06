namespace robot_controller_api.Dtos.Robot;

public class RobotSequenceItemDto
{
    public int Order { get; set; }
    public int CommandId { get; set; }
    public string CommandName { get; set; } = null!;
    public bool Executed { get; set; }
    public DateTime? ExecutedDate { get; set; }
}
