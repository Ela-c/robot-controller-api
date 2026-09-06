namespace robot_controller_api.Dtos.RobotCommands
{
    public class RobotCommandSubmitRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMoveCommand { get; set; }
        public Models.MovementDirection? MovementDirection { get; set; }
    }
}
