namespace robot_controller_api.Services.RobotCommands
{
    public interface IRobotCommandQueue
    {
        ValueTask QueueAsync(int commandId, CancellationToken cancellationToken);
        ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
    }
}
