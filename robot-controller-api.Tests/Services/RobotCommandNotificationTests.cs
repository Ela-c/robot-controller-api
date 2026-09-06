using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Services;

public class RobotCommandNotificationTests
{
    [Fact]
    public async Task SubmitAsync_NotifiesQueuedStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), out var notifier);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();

        var command = await service.SubmitAsync(new RobotCommandSubmitRequestDto { Name = "MOVE", IsMoveCommand = true }, CancellationToken.None);

        var update = Assert.Single(notifier.CommandUpdates);
        Assert.Equal(command.Id, update.CommandId);
        Assert.Equal(RobotCommandStatus.Queued.ToString(), update.Status);
    }

    [Fact]
    public async Task TryStartExecutionAsync_NotifiesExecutingStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), out var notifier);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
        var command = await service.SubmitAsync(new RobotCommandSubmitRequestDto { Name = "REPORT", IsMoveCommand = false }, CancellationToken.None);
        notifier.CommandUpdates.Clear();

        await service.TryStartExecutionAsync(command.Id, CancellationToken.None);

        var update = Assert.Single(notifier.CommandUpdates);
        Assert.Equal(command.Id, update.CommandId);
        Assert.Equal(RobotCommandStatus.Executing.ToString(), update.Status);
    }

    [Fact]
    public async Task MarkCompletedAsync_NotifiesCompletedStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), out var notifier);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
        var command = await service.SubmitAsync(new RobotCommandSubmitRequestDto { Name = "PLACE", IsMoveCommand = false }, CancellationToken.None);
        await service.TryStartExecutionAsync(command.Id, CancellationToken.None);
        notifier.CommandUpdates.Clear();

        await service.MarkCompletedAsync(command.Id, CancellationToken.None);

        var update = Assert.Single(notifier.CommandUpdates);
        Assert.Equal(command.Id, update.CommandId);
        Assert.Equal(RobotCommandStatus.Completed.ToString(), update.Status);
    }

    [Fact]
    public async Task MarkFailedAsync_NotifiesFailedStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), out var notifier);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
        var command = await service.SubmitAsync(new RobotCommandSubmitRequestDto { Name = "FAIL-CMD", IsMoveCommand = false }, CancellationToken.None);
        await service.TryStartExecutionAsync(command.Id, CancellationToken.None);
        notifier.CommandUpdates.Clear();

        await service.MarkFailedAsync(command.Id, "boom", CancellationToken.None);

        var update = Assert.Single(notifier.CommandUpdates);
        Assert.Equal(command.Id, update.CommandId);
        Assert.Equal(RobotCommandStatus.Failed.ToString(), update.Status);
        Assert.Equal("boom", update.FailureReason);
    }

    [Fact]
    public async Task CancelAsync_NotifiesCancelledStatus()
    {
        using var provider = BuildProvider(Guid.NewGuid().ToString(), out var notifier);
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRobotCommandService>();
        var command = await service.SubmitAsync(new RobotCommandSubmitRequestDto { Name = "CANCEL-ME", IsMoveCommand = false }, CancellationToken.None);
        notifier.CommandUpdates.Clear();

        await service.CancelAsync(command.Id, CancellationToken.None);

        var update = Assert.Single(notifier.CommandUpdates);
        Assert.Equal(command.Id, update.CommandId);
        Assert.Equal(RobotCommandStatus.Cancelled.ToString(), update.Status);
    }

    private static ServiceProvider BuildProvider(string dbName, out RecordingNotifier notifier)
    {
        var services = new ServiceCollection();
        notifier = new RecordingNotifier();

        services.AddLogging();
        services.AddDbContext<RobotContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddSingleton<IRobotCommandQueue, RobotCommandQueue>();
        services.AddSingleton<IRobotUpdateNotifier>(notifier);
        services.AddScoped<IRobotCommandService, RobotCommandService>();

        return services.BuildServiceProvider();
    }

    private sealed class RecordingNotifier : IRobotUpdateNotifier
    {
        public List<RobotCommandUpdateDto> CommandUpdates { get; } = new();

        public Task NotifyCommandUpdatedAsync(RobotCommandUpdateDto update, CancellationToken cancellationToken = default)
        {
            CommandUpdates.Add(update);
            return Task.CompletedTask;
        }

        public Task NotifyPositionUpdatedAsync(RobotPositionUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}