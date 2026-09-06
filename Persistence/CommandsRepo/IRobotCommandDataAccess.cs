using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public interface IRobotCommandDataAccess
    {
        int DeleteRobotCommand(int inputId);
        List<RobotCommand> GetMoveCommands();
        RobotCommand? GetRobotCommandById(int inputId);
        RobotCommand? GetRobotCommandByName(string inputName);
        List<RobotCommand> GetRobotCommands();
        RobotCommand? InsertRobotCommand(RobotCommand robotCommand);
        int UpdateRobotCommand(RobotCommand robotCommand);
    }
}
