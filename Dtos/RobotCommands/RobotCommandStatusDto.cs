namespace robot_controller_api.Dtos.RobotCommands
{
    public class RobotCommandStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsMoveCommand { get; set; }
        public Models.MovementDirection? MovementDirection { get; set; }
        public List<Models.MovementDirection> MovementDirections { get; set; } = new();
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
