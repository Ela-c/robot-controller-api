namespace robot_controller_api.Services.Robot;

public class RobotExecutionOptions
{
    public int StepDelayMs { get; set; } = 0;
    public int MinStepDelayMs { get; set; } = 900;
    public int MaxStepDelayMs { get; set; } = 2200;
    public int MinPendingDelayMs { get; set; } = 600;
    public int MaxPendingDelayMs { get; set; } = 1800;
    public int MinQueuedDelayMs { get; set; } = 700;
    public int MaxQueuedDelayMs { get; set; } = 2000;
    public double SequenceFailureProbability { get; set; } = 0.01;
}
