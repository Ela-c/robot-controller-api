namespace robot_controller_api.Dtos.Robot;

public class RobotSequenceSubmissionResponseDto
{
    public int SequenceId { get; set; }
    public string Status { get; set; } = null!;
    public RobotCoordinateDto StartPosition { get; set; } = null!;
    public RobotCoordinateDto PredictedFinalPosition { get; set; } = null!;
    public int TotalSteps { get; set; }
    public string StatusUrl { get; set; } = null!;
}
