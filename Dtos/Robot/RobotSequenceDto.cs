namespace robot_controller_api.Dtos.Robot;

public class RobotSequenceDto
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;
    public int MapId { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int? FinalX { get; set; }
    public int? FinalY { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public bool CancellationRequested { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? FailureReason { get; set; }
    public List<RobotSequenceItemDto> Items { get; set; } = new();
}
