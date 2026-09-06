using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.Robot;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Services;

public class RobotSequenceServiceTests
{
    [Fact]
    public async Task SubmitSequence_ValidRequest_IsPending()
    {
        await using var context = BuildContext();
        var queue = new RecordingSequenceQueue();
        var service = BuildService(context, queue);

        var seed = await SeedBasicData(context);

        var result = await service.SubmitAsync(seed.UserId, new RobotSequenceSubmitRequest
        {
            CommandIds = new[] { seed.MoveRightCommandId, seed.MoveRightCommandId, seed.MoveDownCommandId }
        }, CancellationToken.None);

        Assert.Equal(RobotSequenceStatus.Pending, result.Sequence.Status);
        Assert.Equal(1, queue.QueuedIds.Count);
        Assert.Equal(result.Sequence.Id, queue.QueuedIds[0]);
    }

    [Fact]
    public async Task SubmitSequence_EmptySequence_IsRejected()
    {
        await using var context = BuildContext();
        var service = BuildService(context, new RecordingSequenceQueue());
        var seed = await SeedBasicData(context);

        var ex = await Assert.ThrowsAsync<RobotDomainException>(() => service.SubmitAsync(seed.UserId, new RobotSequenceSubmitRequest
        {
            CommandIds = Array.Empty<int>()
        }, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task SubmitSequence_NonMoveCommand_IsRejected()
    {
        await using var context = BuildContext();
        var service = BuildService(context, new RecordingSequenceQueue());
        var seed = await SeedBasicData(context);

        var ex = await Assert.ThrowsAsync<RobotDomainException>(() => service.SubmitAsync(seed.UserId, new RobotSequenceSubmitRequest
        {
            CommandIds = new[] { seed.ReportCommandId }
        }, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
    }

    [Fact]
    public async Task SubmitSequence_TargetMismatch_IsRejected()
    {
        await using var context = BuildContext();
        var service = BuildService(context, new RecordingSequenceQueue());
        var seed = await SeedBasicData(context);

        var ex = await Assert.ThrowsAsync<RobotDomainException>(() => service.SubmitAsync(seed.UserId, new RobotSequenceSubmitRequest
        {
            CommandIds = new[] { seed.MoveRightCommandId },
            TargetX = 4,
            TargetY = 4
        }, CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, ex.StatusCode);
        Assert.Equal("Movement sequence does not reach target", ex.Title);
    }

    [Fact]
    public async Task ExecuteSequence_PersistsRobotPositionAndCompletes()
    {
        await using var context = BuildContext();
        var service = BuildService(context, new RecordingSequenceQueue());
        var seed = await SeedBasicData(context, startX: 0, startY: 0);

        var submitted = await service.SubmitAsync(seed.UserId, new RobotSequenceSubmitRequest
        {
            CommandIds = new[] { seed.MoveRightCommandId, seed.MoveDownCommandId }
        }, CancellationToken.None);

        var queued = await service.TryQueueExecutionAsync(submitted.Sequence.Id, CancellationToken.None);
        Assert.NotNull(queued);

        var sequence = await service.TryStartExecutionAsync(submitted.Sequence.Id, CancellationToken.None);
        Assert.NotNull(sequence);

        var executor = new RobotSequenceExecutor(
            context,
            new RobotMovementService(),
            new NoOpNotifier(),
            Options.Create(new RobotExecutionOptions { StepDelayMs = 0, SequenceFailureProbability = 0 }),
            NullLogger<RobotSequenceExecutor>.Instance);

        var execution = await executor.ExecuteAsync(sequence!, CancellationToken.None);
        await service.MarkCompletedAsync(sequence!.Id, execution.FinalPosition.X, execution.FinalPosition.Y, CancellationToken.None);

        var state = await context.RobotStates.FirstAsync(s => s.UserId == seed.UserId);
        var reloaded = await service.GetByIdForUserAsync(seed.UserId, sequence.Id, CancellationToken.None);

        Assert.Equal(1, state.X);
        Assert.Equal(1, state.Y);
        Assert.NotNull(reloaded);
        Assert.Equal(RobotSequenceStatus.Completed, reloaded!.Status);
        Assert.Equal(2, reloaded.CurrentStep);
        Assert.Equal(1, reloaded.FinalX);
        Assert.Equal(1, reloaded.FinalY);
    }

    private static RobotContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<RobotContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RobotContext(options);
    }

    private static RobotSequenceService BuildService(RobotContext context, RecordingSequenceQueue queue)
    {
        return new RobotSequenceService(
            context,
            new RobotMovementService(),
            queue,
            new NoOpNotifier(),
            NullLogger<RobotSequenceService>.Instance);
    }

    private static async Task<SeedData> SeedBasicData(RobotContext context, int startX = 1, int startY = 1)
    {
        var now = DateTime.UtcNow;

        var user = new User
        {
            Email = "test@mail.com",
            FirstName = "Test",
            LastName = "User",
            PasswordHash = "pw",
            Role = "user",
            CreatedDate = now,
            ModifiedDate = now
        };

        var map = new Map
        {
            Name = "Main",
            Rows = 5,
            Columns = 5,
            CreatedDate = now,
            ModifiedDate = now
        };

        var moveRight = new RobotCommand
        {
            Name = "Move Right",
            IsMoveCommand = true,
            MovementDirection = MovementDirection.Right,
            CreatedDate = now,
            ModifiedDate = now
        };

        var moveDown = new RobotCommand
        {
            Name = "Move Down",
            IsMoveCommand = true,
            MovementDirection = MovementDirection.Down,
            CreatedDate = now,
            ModifiedDate = now
        };

        var report = new RobotCommand
        {
            Name = "Report",
            IsMoveCommand = false,
            MovementDirection = null,
            CreatedDate = now,
            ModifiedDate = now
        };

        context.Users.Add(user);
        context.Maps.Add(map);
        context.RobotCommands.AddRange(moveRight, moveDown, report);
        await context.SaveChangesAsync();

        context.RobotStates.Add(new RobotState
        {
            UserId = user.Id,
            MapId = map.Id,
            X = startX,
            Y = startY,
            CreatedDate = now,
            ModifiedDate = now
        });

        await context.SaveChangesAsync();

        return new SeedData(user.Id, moveRight.Id, moveDown.Id, report.Id);
    }

    private sealed class RecordingSequenceQueue : IRobotSequenceQueue
    {
        public List<int> QueuedIds { get; } = new();

        public ValueTask QueueAsync(int sequenceId, CancellationToken cancellationToken)
        {
            QueuedIds.Add(sequenceId);
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> DequeueAsync(CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }

    private sealed class NoOpNotifier : IRobotUpdateNotifier
    {
        public Task NotifyCommandUpdatedAsync(RobotCommandUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyPositionUpdatedAsync(RobotPositionUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifySequenceUpdatedAsync(RobotSequenceUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifySequencePositionUpdatedAsync(RobotSequencePositionUpdateDto update, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private readonly record struct SeedData(int UserId, int MoveRightCommandId, int MoveDownCommandId, int ReportCommandId);
}
