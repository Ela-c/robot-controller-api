namespace robot_controller_api.Dtos.Realtime;

public record RobotCommandUpdateDto(
    int CommandId,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime UpdatedAt,
    string? FailureReason);