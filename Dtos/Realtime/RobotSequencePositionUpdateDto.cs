namespace robot_controller_api.Dtos.Realtime;

public record RobotSequencePositionUpdateDto(
    int SequenceId,
    int MapId,
    int X,
    int Y,
    int Step,
    int TotalSteps,
    DateTime Timestamp);
