namespace robot_controller_api.Hubs;

public static class RobotHubGroups
{
    public static string Command(int commandId) => $"command-{commandId}";
    public static string Sequence(int sequenceId) => $"sequence-{sequenceId}";
}