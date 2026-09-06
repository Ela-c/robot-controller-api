using System.Threading.Channels;

namespace robot_controller_api.Services.Robot;

public class RobotSequenceQueue : IRobotSequenceQueue
{
    private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();

    public async ValueTask QueueAsync(int sequenceId, CancellationToken cancellationToken)
    {
        await _queue.Writer.WriteAsync(sequenceId, cancellationToken);
    }

    public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
