using Microsoft.AspNetCore.SignalR;
using robot_controller_api.Dtos.Realtime;
using robot_controller_api.Hubs;

namespace robot_controller_api.Services.RobotCommands;

public class SignalRRobotUpdateNotifier : IRobotUpdateNotifier
{
    private readonly IHubContext<RobotHub> _hubContext;
    private readonly ILogger<SignalRRobotUpdateNotifier> _logger;

    public SignalRRobotUpdateNotifier(
        IHubContext<RobotHub> hubContext,
        ILogger<SignalRRobotUpdateNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyCommandUpdatedAsync(RobotCommandUpdateDto update, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(RobotHubGroups.Command(update.CommandId))
                .SendAsync(RobotHubEvents.RobotCommandUpdated, update, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send command update over SignalR for command {CommandId}",
                update.CommandId);
        }
    }

    public async Task NotifyPositionUpdatedAsync(RobotPositionUpdateDto update, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(RobotHubGroups.Command(update.CommandId))
                .SendAsync(RobotHubEvents.RobotPositionUpdated, update, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send position update over SignalR for command {CommandId}",
                update.CommandId);
        }
    }

    public async Task NotifySequenceUpdatedAsync(RobotSequenceUpdateDto update, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(RobotHubGroups.Sequence(update.SequenceId))
                .SendAsync(RobotHubEvents.RobotSequenceUpdated, update, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send sequence update over SignalR for sequence {SequenceId}",
                update.SequenceId);
        }
    }

    public async Task NotifySequencePositionUpdatedAsync(RobotSequencePositionUpdateDto update, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(RobotHubGroups.Sequence(update.SequenceId))
                .SendAsync(RobotHubEvents.RobotPositionUpdated, update, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to send sequence position update over SignalR for sequence {SequenceId}",
                update.SequenceId);
        }
    }
}