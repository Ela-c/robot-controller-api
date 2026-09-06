namespace robot_controller_api.Models
{
    public class RobotState
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? MapId { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public User User { get; set; } = null!;
        public Map? Map { get; set; }
    }
}
