using Microsoft.AspNetCore.SignalR;
using robot_controller_api.Services.RobotCommands;

namespace robot_controller_api.Hubs;

public class RobotHub : Hub
{
    private readonly IRobotCommandService _robotCommandService;
    private readonly ILogger<RobotHub> _logger;

    public RobotHub(IRobotCommandService robotCommandService, ILogger<RobotHub> logger)
    {
        _robotCommandService = robotCommandService;
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
}