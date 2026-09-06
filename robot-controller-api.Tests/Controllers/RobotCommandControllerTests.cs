using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Controllers;
using robot_controller_api.Dtos.RobotCommands;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Tests.Controllers;

public class RobotCommandControllerTests
{
    [Fact]
    public async Task AddRobotCommand_WhenCreated_ReturnsCreatedAtRoute()
    {
        var submitted = new RobotCommand
        {
            Id = 8,
            Name = "MOVE",
            IsMoveCommand = true,
            MovementDirection = MovementDirection.Right,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        var service = new FakeRobotCommandService
        {
            SubmitFunc = (_, _) => Task.FromResult(submitted)
        };

        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.AddRobotCommand(new RobotCommandSubmitRequestDto { Name = "MOVE", IsMoveCommand = true }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRouteResult>(result);
        Assert.Equal("GetRobotCommand", created.RouteName);

        var payload = Assert.IsType<RobotCommandSubmissionResponseDto>(created.Value);
        Assert.Equal(8, payload.Id);
        Assert.Equal("/api/robot-commands/8", payload.CommandUrl);
    }

    [Fact]
    public async Task CancelRobotCommand_WhenDefinitionExists_ReturnsConflict()
    {
        var service = new FakeRobotCommandService
        {
            CancelFunc = (_, _) => Task.FromResult(RobotCommandCancellationResult.NotAllowed)
        };

        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.CancelRobotCommand(5, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("command definitions cannot be cancelled", conflict.Value);
    }

    private sealed class FakeRobotCommandRepo : IRobotCommandDataAccess
    {
        public int DeleteRobotCommand(int inputId) => 1;
        public List<RobotCommand> GetMoveCommands() => [];
        public RobotCommand? GetRobotCommandById(int inputId) => null;
        public RobotCommand? GetRobotCommandByName(string inputName) => null;
        public List<RobotCommand> GetRobotCommands() => [];
        public RobotCommand? InsertRobotCommand(RobotCommand robotCommand) => robotCommand;
        public int UpdateRobotCommand(RobotCommand robotCommand) => 1;
    }

    private sealed class FakeRobotCommandService : IRobotCommandService
    {
        public Func<RobotCommandSubmitRequestDto, CancellationToken, Task<RobotCommand>> SubmitFunc { get; set; }
            = (_, _) => Task.FromResult(new RobotCommand());

        public Func<int, RobotCommandSubmitRequestDto, CancellationToken, Task<RobotCommand?>> UpdateFunc { get; set; }
            = (_, _, _) => Task.FromResult<RobotCommand?>(null);

        public Func<int, CancellationToken, Task<RobotCommandStatusDto?>> GetStatusFunc { get; set; }
            = (_, _) => Task.FromResult<RobotCommandStatusDto?>(null);

        public Func<int, CancellationToken, Task<RobotCommandCancellationResult>> CancelFunc { get; set; }
            = (_, _) => Task.FromResult(RobotCommandCancellationResult.NotAllowed);

        public Task<RobotCommand> SubmitAsync(RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
            => SubmitFunc(request, cancellationToken);

        public Task<RobotCommand?> UpdateAsync(int id, RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
            => UpdateFunc(id, request, cancellationToken);

        public Task<RobotCommandStatusDto?> GetStatusAsync(int id, CancellationToken cancellationToken)
            => GetStatusFunc(id, cancellationToken);

        public Task<RobotCommandCancellationResult> CancelAsync(int id, CancellationToken cancellationToken)
            => CancelFunc(id, cancellationToken);

        public Task<RobotCommand?> TryStartExecutionAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult<RobotCommand?>(null);

        public Task MarkCompletedAsync(int id, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task MarkFailedAsync(int id, string failureReason, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
