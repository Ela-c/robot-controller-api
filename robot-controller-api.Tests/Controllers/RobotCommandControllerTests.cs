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
    public void GetAllRobotCommands_ReturnsRepoCommands()
    {
        var expected = new List<RobotCommand> { CreateCommand(1, "LEFT", true) };
        var repo = new FakeRobotCommandRepo { GetRobotCommandsFunc = () => expected };
        var controller = new RobotCommandController(repo, new FakeRobotCommandService());

        var result = controller.GetAllRobotCommands();

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetRobotCommand_WhenExists_ReturnsOk()
    {
        var status = new RobotCommandStatusDto { Id = 5, Name = "PLACE", Status = "Executing" };
        var service = new FakeRobotCommandService { GetStatusFunc = (_, _) => Task.FromResult<RobotCommandStatusDto?>(status) };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.GetRobotCommand(5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(status, ok.Value);
    }

    [Fact]
    public async Task GetRobotCommand_WhenMissing_ReturnsNotFound()
    {
        var service = new FakeRobotCommandService { GetStatusFunc = (_, _) => Task.FromResult<RobotCommandStatusDto?>(null) };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.GetRobotCommand(100, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddRobotCommand_WhenNull_ReturnsBadRequest()
    {
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), new FakeRobotCommandService());

        var result = await controller.AddRobotCommand(null, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("no command provided", badRequest.Value);
    }

    [Fact]
    public async Task AddRobotCommand_WhenNameInvalid_ReturnsBadRequest()
    {
        var service = new FakeRobotCommandService
        {
            SubmitFunc = (_, _) => throw new ArgumentException("no command name provided")
        };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.AddRobotCommand(new RobotCommandSubmitRequestDto { Name = "" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("no command name provided", badRequest.Value);
    }

    [Fact]
    public async Task AddRobotCommand_WhenAlreadyExists_ReturnsConflict()
    {
        var service = new FakeRobotCommandService
        {
            SubmitFunc = (_, _) => throw new InvalidOperationException("command already exists")
        };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.AddRobotCommand(new RobotCommandSubmitRequestDto { Name = "REPORT" }, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("command already exists", conflict.Value);
    }

    [Fact]
    public async Task AddRobotCommand_WhenAccepted_ReturnsAcceptedAtRoute()
    {
        var submitted = CreateCommand(8, "MOVE", true);
        submitted.Status = RobotCommandStatus.Queued;

        var service = new FakeRobotCommandService
        {
            SubmitFunc = (_, _) => Task.FromResult(submitted)
        };

        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.AddRobotCommand(new RobotCommandSubmitRequestDto { Name = "MOVE", IsMoveCommand = true }, CancellationToken.None);

        var accepted = Assert.IsType<AcceptedAtRouteResult>(result);
        Assert.Equal("GetRobotCommand", accepted.RouteName);

        var payload = Assert.IsType<RobotCommandSubmissionResponseDto>(accepted.Value);
        Assert.Equal(8, payload.Id);
        Assert.Equal("Queued", payload.Status);
        Assert.Equal("/api/robot-commands/8", payload.StatusUrl);
    }

    [Fact]
    public async Task CancelRobotCommand_WhenNotFound_ReturnsNotFound()
    {
        var service = new FakeRobotCommandService
        {
            CancelFunc = (_, _) => Task.FromResult(RobotCommandCancellationResult.NotFound)
        };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.CancelRobotCommand(5, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CancelRobotCommand_WhenNotAllowed_ReturnsConflict()
    {
        var service = new FakeRobotCommandService
        {
            CancelFunc = (_, _) => Task.FromResult(RobotCommandCancellationResult.NotAllowed)
        };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.CancelRobotCommand(5, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CancelRobotCommand_WhenAllowed_ReturnsOk()
    {
        var service = new FakeRobotCommandService
        {
            CancelFunc = (_, _) => Task.FromResult(RobotCommandCancellationResult.Cancelled)
        };
        var controller = new RobotCommandController(new FakeRobotCommandRepo(), service);

        var result = await controller.CancelRobotCommand(5, CancellationToken.None);

        Assert.IsType<OkResult>(result);
    }

    private static RobotCommand CreateCommand(int id, string name, bool isMove) =>
        new(id, name, "desc", isMove, DateTime.UtcNow, DateTime.UtcNow);

    private sealed class FakeRobotCommandRepo : IRobotCommandDataAccess
    {
        public Func<int, int> DeleteFunc { get; set; } = _ => 1;
        public Func<List<RobotCommand>> GetMoveCommandsFunc { get; set; } = () => new List<RobotCommand>();
        public Func<int, RobotCommand?> GetByIdFunc { get; set; } = _ => null;
        public Func<string, RobotCommand?> GetByNameFunc { get; set; } = _ => null;
        public Func<List<RobotCommand>> GetRobotCommandsFunc { get; set; } = () => new List<RobotCommand>();
        public Func<RobotCommand, RobotCommand?> InsertFunc { get; set; } = command => command;
        public Func<RobotCommand, int> UpdateFunc { get; set; } = _ => 1;

        public int DeleteRobotCommand(int inputId) => DeleteFunc(inputId);
        public List<RobotCommand> GetMoveCommands() => GetMoveCommandsFunc();
        public RobotCommand? GetRobotCommandById(int inputId) => GetByIdFunc(inputId);
        public RobotCommand? GetRobotCommandByName(string inputName) => GetByNameFunc(inputName);
        public List<RobotCommand> GetRobotCommands() => GetRobotCommandsFunc();
        public RobotCommand? InsertRobotCommand(RobotCommand robotCommand) => InsertFunc(robotCommand);
        public int UpdateRobotCommand(RobotCommand robotCommand) => UpdateFunc(robotCommand);
    }

    private sealed class FakeRobotCommandService : IRobotCommandService
    {
        public Func<RobotCommandSubmitRequestDto, CancellationToken, Task<RobotCommand>> SubmitFunc { get; set; }
            = (_, _) => Task.FromResult(new RobotCommand());

        public Func<int, CancellationToken, Task<RobotCommandStatusDto?>> GetStatusFunc { get; set; }
            = (_, _) => Task.FromResult<RobotCommandStatusDto?>(null);

        public Func<int, CancellationToken, Task<RobotCommandCancellationResult>> CancelFunc { get; set; }
            = (_, _) => Task.FromResult(RobotCommandCancellationResult.Cancelled);

        public Func<int, CancellationToken, Task<RobotCommand?>> TryStartExecutionFunc { get; set; }
            = (_, _) => Task.FromResult<RobotCommand?>(null);

        public Func<int, CancellationToken, Task> MarkCompletedFunc { get; set; }
            = (_, _) => Task.CompletedTask;

        public Func<int, string, CancellationToken, Task> MarkFailedFunc { get; set; }
            = (_, _, _) => Task.CompletedTask;

        public Task<RobotCommand> SubmitAsync(RobotCommandSubmitRequestDto request, CancellationToken cancellationToken)
            => SubmitFunc(request, cancellationToken);

        public Task<RobotCommandStatusDto?> GetStatusAsync(int id, CancellationToken cancellationToken)
            => GetStatusFunc(id, cancellationToken);

        public Task<RobotCommandCancellationResult> CancelAsync(int id, CancellationToken cancellationToken)
            => CancelFunc(id, cancellationToken);

        public Task<RobotCommand?> TryStartExecutionAsync(int id, CancellationToken cancellationToken)
            => TryStartExecutionFunc(id, cancellationToken);

        public Task MarkCompletedAsync(int id, CancellationToken cancellationToken)
            => MarkCompletedFunc(id, cancellationToken);

        public Task MarkFailedAsync(int id, string failureReason, CancellationToken cancellationToken)
            => MarkFailedFunc(id, failureReason, cancellationToken);
    }
}
