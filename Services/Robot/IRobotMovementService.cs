using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public interface IRobotMovementService
{
    RobotPosition CalculateNextPosition(RobotPosition current, MovementDirection direction);
    bool IsWithinMap(Map map, RobotPosition position);
    SequenceValidationResult ValidateSequence(Map map, RobotPosition start, IReadOnlyList<RobotCommand> commands, int? targetX, int? targetY);
}
