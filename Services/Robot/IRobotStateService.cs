using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public interface IRobotStateService
{
    Task<RobotState> GetOrCreateAsync(int userId, CancellationToken cancellationToken);
    Task<RobotState> SelectMapAsync(int userId, int mapId, CancellationToken cancellationToken);
    Task<RobotState> PlaceRobotAsync(int userId, int x, int y, CancellationToken cancellationToken);
}
