using robot_controller_api.Models;

namespace robot_controller_api.Services.Robot;

public class RobotMovementService : IRobotMovementService
{
    public RobotPosition CalculateNextPosition(RobotPosition current, MovementDirection direction)
    {
        return direction switch
        {
            MovementDirection.Up => new RobotPosition(current.X, current.Y - 1),
            MovementDirection.Down => new RobotPosition(current.X, current.Y + 1),
            MovementDirection.Left => new RobotPosition(current.X - 1, current.Y),
            MovementDirection.Right => new RobotPosition(current.X + 1, current.Y),
            _ => current
        };
    }

    public bool IsWithinMap(Map map, RobotPosition position)
    {
        return position.X >= 0 && position.X < map.Columns && position.Y >= 0 && position.Y < map.Rows;
    }

    public SequenceValidationResult ValidateSequence(Map map, RobotPosition start, IReadOnlyList<RobotCommand> commands, int? targetX, int? targetY)
    {
        var current = start;

        for (var index = 0; index < commands.Count; index++)
        {
            var command = commands[index];
            var step = index + 1;

            if (!command.IsMoveCommand)
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    FailingStep = step,
                    FailingPosition = current,
                    FailingCommand = command,
                    ErrorTitle = "Invalid movement sequence",
                    ErrorDetail = $"Step {step} ({command.Name}) is not a movement command."
                };
            }

            if (command.MovementDirection == null)
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    FailingStep = step,
                    FailingPosition = current,
                    FailingCommand = command,
                    ErrorTitle = "Invalid movement sequence",
                    ErrorDetail = $"Step {step} ({command.Name}) has no movement direction."
                };
            }

            var next = CalculateNextPosition(current, command.MovementDirection.Value);
            if (!IsWithinMap(map, next))
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    FailingStep = step,
                    FailingPosition = current,
                    FailingCommand = command,
                    ErrorTitle = "Invalid movement sequence",
                    ErrorDetail = $"Step {step} ({command.Name}) would move the robot outside the map."
                };
            }

            current = next;
        }

        if (targetX.HasValue || targetY.HasValue)
        {
            if (!targetX.HasValue || !targetY.HasValue)
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    ErrorTitle = "Invalid target",
                    ErrorDetail = "Both targetX and targetY must be provided together."
                };
            }

            var target = new RobotPosition(targetX.Value, targetY.Value);
            if (!IsWithinMap(map, target))
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    ErrorTitle = "Invalid target",
                    ErrorDetail = "The target coordinate is outside the selected map."
                };
            }

            if (target != current)
            {
                return new SequenceValidationResult
                {
                    IsValid = false,
                    FinalPosition = current,
                    ErrorTitle = "Movement sequence does not reach target",
                    ErrorDetail = $"The submitted sequence ends at ({current.X},{current.Y}), but the requested target is ({target.X},{target.Y})."
                };
            }
        }

        return new SequenceValidationResult
        {
            IsValid = true,
            FinalPosition = current
        };
    }
}
