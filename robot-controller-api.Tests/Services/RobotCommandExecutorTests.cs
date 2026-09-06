using Microsoft.Extensions.Logging.Abstractions;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Models;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Services;

public class RobotCommandExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_MoveCommand_PublishesIncrementalPositionUpdates()
    {
        var notifier = new RecordingNotifier();
        var executor = new RobotCommandExecutor(notifier, NullLogger<RobotCommandExecutor>.Instance);
        var command = new RobotCommand
        {
            Id = 42,
            Name = "MOVE",
            IsMoveCommand = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await executor.ExecuteAsync(command, CancellationToken.None);

        Assert.Equal(3, notifier.PositionUpdates.Count);
        Assert.All(notifier.PositionUpdates, update => Assert.Equal(42, update.CommandId));
        Assert.Equal(new[] { 1, 2, 3 }, notifier.PositionUpdates.Select(update => update.X));
    }

    [Fact]
    public async Task ExecuteAsync_NonMoveCommand_DoesNotPublishPositionUpdates()
    {
        var notifier = new RecordingNotifier();
        var executor = new RobotCommandExecutor(notifier, NullLogger<RobotCommandExecutor>.Instance);
        var command = new RobotCommand
        {
            Id = 7,
            Name = "REPORT",
            IsMoveCommand = false,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        await executor.ExecuteAsync(command, CancellationToken.None);

        Assert.Empty(notifier.PositionUpdates);
    }

    private sealed class RecordingNotifier : IRobotUpdateNotifier
    {
        public List<RobotPositionUpdateDto> PositionUpdates { get; } = new();

        public Task NotifyCommandUpdatedAsync(RobotCommandUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyPositionUpdatedAsync(RobotPositionUpdateDto update, CancellationToken cancellationToken = default)
        {
            PositionUpdates.Add(update);
            return Task.CompletedTask;
        }

        public Task NotifySequenceUpdatedAsync(RobotSequenceUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifySequencePositionUpdatedAsync(RobotSequencePositionUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}