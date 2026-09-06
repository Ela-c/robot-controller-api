namespace robot_controller_api.Models
{
    public class RobotCommandSequence
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MapId { get; set; }
        public RobotSequenceStatus Status { get; set; }
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
        public DateTime ModifiedDate { get; set; }
        public string? FailureReason { get; set; }
        public User User { get; set; } = null!;
        public Map Map { get; set; } = null!;
        public ICollection<RobotCommandSequenceItem> Items { get; set; } = new List<RobotCommandSequenceItem>();
    }
}
