using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Services.Robot;

public class RobotStateService : IRobotStateService
{
    private readonly RobotContext _context;
    private readonly IRobotMovementService _robotMovementService;
    private readonly ILogger<RobotStateService> _logger;

    public RobotStateService(RobotContext context, IRobotMovementService robotMovementService, ILogger<RobotStateService> logger)
    {
        _context = context;
        _robotMovementService = robotMovementService;
        _logger = logger;
    }

    public async Task<RobotState> GetOrCreateAsync(int userId, CancellationToken cancellationToken)
    {
        var state = await _context.RobotStates
            .Include(s => s.Map)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (state != null)
        {
            return state;
        }

        var now = DateTime.UtcNow;
        state = new RobotState
        {
            UserId = userId,
            CreatedDate = now,
            ModifiedDate = now
        };

        _context.RobotStates.Add(state);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Robot state created for user {UserId}", userId);
        return state;
    }

    public async Task<RobotState> SelectMapAsync(int userId, int mapId, CancellationToken cancellationToken)
    {
        var map = await _context.Maps.FirstOrDefaultAsync(m => m.Id == mapId, cancellationToken);
        if (map == null)
        {
            throw new RobotDomainException(StatusCodes.Status404NotFound, "Map not found", $"Map {mapId} was not found.");
        }

        var state = await GetOrCreateAsync(userId, cancellationToken);
        state.MapId = mapId;
        state.Map = map;
        state.X = null;
        state.Y = null;
        state.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Robot map selected for user {UserId}: map {MapId}", userId, mapId);
        return state;
    }

    public async Task<RobotState> PlaceRobotAsync(int userId, int x, int y, CancellationToken cancellationToken)
    {
        var state = await GetOrCreateAsync(userId, cancellationToken);

        if (state.MapId == null)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "No map selected", "Select a map before placing or moving the robot.");
        }

        var map = await _context.Maps.FirstOrDefaultAsync(m => m.Id == state.MapId.Value, cancellationToken);
        if (map == null)
        {
            throw new RobotDomainException(StatusCodes.Status409Conflict, "Selected map not found", "The selected map no longer exists.");
        }

        var position = new RobotPosition(x, y);
        if (!_robotMovementService.IsWithinMap(map, position))
        {
            throw new RobotDomainException(StatusCodes.Status400BadRequest, "Invalid robot position", "The supplied coordinate is outside map boundaries.")
                .WithExtension("x", x)
                .WithExtension("y", y);
        }

        state.Map = map;
        state.X = x;
        state.Y = y;
        state.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Robot placed for user {UserId} at ({X},{Y}) on map {MapId}", userId, x, y, state.MapId);
        return state;
    }
}
