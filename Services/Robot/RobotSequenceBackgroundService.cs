namespace robot_controller_api.Services.Robot;

public class RobotSequenceBackgroundService : BackgroundService
{
    private readonly IRobotSequenceQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RobotSequenceBackgroundService> _logger;

    public RobotSequenceBackgroundService(
        IRobotSequenceQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<RobotSequenceBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int sequenceId;
            try
            {
                sequenceId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            using var scope = _scopeFactory.CreateScope();
            var sequenceService = scope.ServiceProvider.GetRequiredService<IRobotSequenceService>();
            var executor = scope.ServiceProvider.GetRequiredService<IRobotSequenceExecutor>();

            try
            {
                var sequence = await sequenceService.TryStartExecutionAsync(sequenceId, stoppingToken);
                if (sequence == null)
                {
                    _logger.LogInformation("Sequence {SequenceId} skipped because it is no longer executable", sequenceId);
                    continue;
                }

                var result = await executor.ExecuteAsync(sequence, stoppingToken);
                if (result.Cancelled)
                {
                    continue;
                }

                await sequenceService.MarkCompletedAsync(sequenceId, result.FinalPosition.X, result.FinalPosition.Y, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Robot sequence {SequenceId} failed during execution", sequenceId);

                var failureReason = string.IsNullOrWhiteSpace(exception.Message)
                    ? "Unknown execution failure"
                    : exception.Message;

                await sequenceService.MarkFailedAsync(sequenceId, failureReason, stoppingToken);
            }
        }
    }
}
