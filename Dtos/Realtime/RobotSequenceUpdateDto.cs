namespace robot_controller_api.Dtos.Realtime;

public record RobotSequenceUpdateDto(
    int SequenceId,
    string Status,
    int CurrentStep,
    int TotalSteps,
    DateTime Timestamp,
    string? FailureReason);
