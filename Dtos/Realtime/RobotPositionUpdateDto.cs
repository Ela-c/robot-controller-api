namespace robot_controller_api.Dtos.Realtime;

public record RobotPositionUpdateDto(
    int CommandId,
    int X,
    int Y,
    DateTime Timestamp);