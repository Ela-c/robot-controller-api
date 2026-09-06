namespace robot_controller_api.Dtos.Robot;

public class RobotStateDto
{
    public int? MapId { get; set; }
    public string? MapName { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public bool HasPosition { get; set; }
    public DateTime ModifiedDate { get; set; }
}
