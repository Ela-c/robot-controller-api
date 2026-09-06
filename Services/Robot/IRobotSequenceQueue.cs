namespace robot_controller_api.Services.Robot;

public interface IRobotSequenceQueue
{
    ValueTask QueueAsync(int sequenceId, CancellationToken cancellationToken);
    ValueTask<int> DequeueAsync(CancellationToken cancellationToken);
}
