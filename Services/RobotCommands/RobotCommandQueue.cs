using System.Threading.Channels;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandQueue : IRobotCommandQueue
    {
        private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();

        public async ValueTask QueueAsync(int commandId, CancellationToken cancellationToken)
        {
            await _queue.Writer.WriteAsync(commandId, cancellationToken);
        }

        public async ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
