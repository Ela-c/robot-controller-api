namespace robot_controller_api.Dtos.RobotCommands
{
    public class RobotCommandSubmissionResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public string StatusUrl { get; set; } = null!;
    }
}
