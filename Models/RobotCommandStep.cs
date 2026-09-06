namespace robot_controller_api.Models
{
    public class RobotCommandStep
    {
        public int Id { get; set; }
        public int RobotCommandId { get; set; }
        public int Order { get; set; }
        public MovementDirection MovementDirection { get; set; }
        public RobotCommand RobotCommand { get; set; } = null!;
    }
}
