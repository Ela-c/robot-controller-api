using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Services;

public class RobotCommandWorkflowTests
{
    [Fact]
    public async Task SubmitCommand_CreatesCommandWithQueuedStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), new ControlledExecutor());
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();

        var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
        {
            Name = "MOVE",
            IsMoveCommand = true
        }, CancellationToken.None);

        Assert.True(created.Id > 0);
        Assert.Equal(RobotCommandStatus.Queued, created.Status);
    }

    [Fact]
    public async Task QueuedCommand_TransitionsToExecutingThenCompleted()
    {
        var executor = new ControlledExecutor();
        using var provider = BuildProvider(Guid.NewGuid().ToString(), executor);

        int commandId;
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
            {
                Name = "RIGHT",
                IsMoveCommand = true
            }, CancellationToken.None);
            commandId = created.Id;
        }

        var worker = new RobotCommandBackgroundService(
            provider.GetRequiredService<IRobotCommandQueue>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<RobotCommandBackgroundService>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await executor.ExecutionStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var status = await service.GetStatusAsync(commandId, CancellationToken.None);
            Assert.NotNull(status);
            Assert.Equal("Executing", status!.Status);
        }

        executor.ReleaseSuccess();
        await WaitForStatusAsync(provider, commandId, RobotCommandStatus.Completed, TimeSpan.FromSeconds(2));
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task FailedExecution_TransitionsToFailedAndPersistsReason()
    {
        var executor = new ControlledExecutor();
        using var provider = BuildProvider(Guid.NewGuid().ToString(), executor);

        int commandId;
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
            {
                Name = "FAIL-CMD",
                IsMoveCommand = false
            }, CancellationToken.None);
            commandId = created.Id;
        }

        var worker = new RobotCommandBackgroundService(
            provider.GetRequiredService<IRobotCommandQueue>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<RobotCommandBackgroundService>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await executor.ExecutionStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        executor.ReleaseFailure(new InvalidOperationException("Simulated worker failure"));

        var failed = await WaitForStatusAsync(provider, commandId, RobotCommandStatus.Failed, TimeSpan.FromSeconds(2));
        Assert.Equal("Simulated worker failure", failed.FailureReason);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CancelQueuedCommand_SetsCancelledStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), new ControlledExecutor());

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
        var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
        {
            Name = "REPORT",
            IsMoveCommand = false
        }, CancellationToken.None);

        var result = await service.CancelAsync(created.Id, CancellationToken.None);
        var status = await service.GetStatusAsync(created.Id, CancellationToken.None);

        Assert.Equal(RobotCommandCancellationResult.Cancelled, result);
        Assert.NotNull(status);
        Assert.Equal("Cancelled", status!.Status);
    }

    [Fact]
    public async Task CompletedCommand_CannotBeCancelled()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), new ControlledExecutor());

        int commandId;
        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var created = await service.SubmitAsync(new RobotCommandSubmitRequestDto
            {
                Name = "PLACE",
                IsMoveCommand = true
            }, CancellationToken.None);

            commandId = created.Id;
            await service.TryStartExecutionAsync(commandId, CancellationToken.None);
            await service.MarkCompletedAsync(commandId, CancellationToken.None);
        }

        using (var scope = provider.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var cancelResult = await service.CancelAsync(commandId, CancellationToken.None);

            Assert.Equal(RobotCommandCancellationResult.NotAllowed, cancelResult);
        }
    }

    private static ServiceProvider BuildProvider(string dbName, ControlledExecutor executor)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<RobotContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IRobotCommandService, RobotCommandService>();
        services.AddSingleton<IRobotCommandQueue, RobotCommandQueue>();
        services.AddSingleton<IRobotUpdateNotifier, NoOpNotifier>();
        services.AddScoped<IRobotCommandExecutor>(_ => executor);

        return services.BuildServiceProvider();
    }

    private static async Task<RobotCommandStatusDto> WaitForStatusAsync(
        ServiceProvider provider,
        int commandId,
        RobotCommandStatus expectedStatus,
        TimeSpan timeout)
    {
        var started = DateTime.UtcNow;
        while (DateTime.UtcNow - started < timeout)
        {
            using var scope = provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
            var status = await service.GetStatusAsync(commandId, CancellationToken.None);
            if (status != null && status.Status == expectedStatus.ToString())
            {
                return status;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Command {commandId} did not reach {expectedStatus} in time.");
    }

    private sealed class ControlledExecutor : IRobotCommandExecutor
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource ExecutionStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Exception? Failure { get; private set; }

        public async Task ExecuteAsync(RobotCommand command, CancellationToken cancellationToken)
        {
            ExecutionStarted.TrySetResult();
            await _release.Task.WaitAsync(cancellationToken);

            if (Failure != null)
            {
                throw Failure;
            }
        }

        public void ReleaseSuccess()
        {
            _release.TrySetResult();
        }

        public void ReleaseFailure(Exception exception)
        {
            Failure = exception;
            _release.TrySetResult();
        }
    }

    private sealed class NoOpNotifier : IRobotUpdateNotifier
    {
        public Task NotifyCommandUpdatedAsync(Dtos.Realtime.RobotCommandUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyPositionUpdatedAsync(Dtos.Realtime.RobotPositionUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
