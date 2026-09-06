namespace robot_controller_api.Models
{
    public class RobotCommandSequenceItem
    {
        public int Id { get; set; }
        public int SequenceId { get; set; }
        public int RobotCommandId { get; set; }
        public int Order { get; set; }
        public bool IsExecuted { get; set; }
        public DateTime? ExecutedDate { get; set; }
        public string CommandName { get; set; } = null!;
        public MovementDirection MovementDirection { get; set; }
        public RobotCommandSequence Sequence { get; set; } = null!;
        public RobotCommand RobotCommand { get; set; } = null!;
    }
}
