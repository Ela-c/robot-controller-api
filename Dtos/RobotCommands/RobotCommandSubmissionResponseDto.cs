namespace robot_controller_api.Dtos.RobotCommands
{
    public class RobotCommandSubmissionResponseDto
    {
        public int Id { get; set; }
        public string CommandUrl { get; set; } = null!;
    }
}
