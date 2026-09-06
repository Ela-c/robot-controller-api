using Microsoft.AspNetCore.SignalR;
using robot_controller_api.Services.RobotCommands;
using robot_controller_api.Services.Robot;
using System.Security.Claims;

namespace robot_controller_api.Hubs;

public class RobotHub : Hub
{
    private readonly IRobotCommandService _robotCommandService;
    private readonly IRobotSequenceService _robotSequenceService;
    private readonly ILogger<RobotHub> _logger;

    public RobotHub(IRobotCommandService robotCommandService, IRobotSequenceService robotSequenceService, ILogger<RobotHub> logger)
    {
        _robotCommandService = robotCommandService;
        _robotSequenceService = robotSequenceService;
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("SignalR connection established: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("SignalR connection closed: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToCommand(int commandId)
    {
        if (commandId <= 0)
        {
            throw new HubException("Command ID must be a positive integer.");
        }

        var command = await _robotCommandService.GetStatusAsync(commandId, Context.ConnectionAborted);
        if (command == null)
        {
            throw new HubException($"Command {commandId} was not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RobotHubGroups.Command(commandId), Context.ConnectionAborted);

        _logger.LogInformation(
            "SignalR connection {ConnectionId} subscribed to command {CommandId}",
            Context.ConnectionId,
            commandId);
    }

    public async Task UnsubscribeFromCommand(int commandId)
    {
        if (commandId <= 0)
        {
            throw new HubException("Command ID must be a positive integer.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RobotHubGroups.Command(commandId), Context.ConnectionAborted);

        _logger.LogInformation(
            "SignalR connection {ConnectionId} unsubscribed from command {CommandId}",
            Context.ConnectionId,
            commandId);
    }

    public async Task SubscribeToSequence(int sequenceId)
    {
        if (sequenceId <= 0)
        {
            throw new HubException("Sequence ID must be a positive integer.");
        }

        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new HubException("Authentication required.");
        }

        var sequence = await _robotSequenceService.GetByIdForUserAsync(userId, sequenceId, Context.ConnectionAborted);
        if (sequence == null)
        {
            throw new HubException($"Sequence {sequenceId} was not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RobotHubGroups.Sequence(sequenceId), Context.ConnectionAborted);

        _logger.LogInformation(
            "SignalR connection {ConnectionId} subscribed to sequence {SequenceId}",
            Context.ConnectionId,
            sequenceId);
    }

    public async Task UnsubscribeFromSequence(int sequenceId)
    {
        if (sequenceId <= 0)
        {
            throw new HubException("Sequence ID must be a positive integer.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RobotHubGroups.Sequence(sequenceId), Context.ConnectionAborted);

        _logger.LogInformation(
            "SignalR connection {ConnectionId} unsubscribed from sequence {SequenceId}",
            Context.ConnectionId,
            sequenceId);
    }
}