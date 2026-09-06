namespace robot_controller_api.Models
{
    public enum RobotCommandStatus
    {
        Pending = 0,
        Queued = 1,
        Executing = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }
}
