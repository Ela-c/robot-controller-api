using Microsoft.Extensions.Hosting;

namespace robot_controller_api.Services.RobotCommands
{
    public class RobotCommandBackgroundService : BackgroundService
    {
        private readonly IRobotCommandQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RobotCommandBackgroundService> _logger;

        public RobotCommandBackgroundService(
            IRobotCommandQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<RobotCommandBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int commandId;
                try
                {
                    commandId = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                using var scope = _scopeFactory.CreateScope();
                var commandService = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
                var executor = scope.ServiceProvider.GetRequiredService<IRobotCommandExecutor>();

                try
                {
                    var command = await commandService.TryStartExecutionAsync(commandId, stoppingToken);
                    if (command == null)
                    {
                        _logger.LogInformation("Robot command {CommandId} was skipped because it is no longer executable", commandId);
                        continue;
                    }

                    await executor.ExecuteAsync(command, stoppingToken);
                    await commandService.MarkCompletedAsync(commandId, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Robot command {CommandId} failed during execution",
                        commandId);

                    var failureReason = string.IsNullOrWhiteSpace(exception.Message)
                        ? "Unknown execution failure"
                        : exception.Message;

                    await commandService.MarkFailedAsync(commandId, failureReason, stoppingToken);
                }
            }
        }
    }
}
