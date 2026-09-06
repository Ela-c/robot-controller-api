using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public class RobotCommandEF(RobotContext context) : IRobotCommandDataAccess, IDisposable
    {
        private readonly RobotContext _context = context;

        public int DeleteRobotCommand(int inputId)
        {
			RobotCommand? cmdToDelete = _context.RobotCommands.FirstOrDefault(cmd => cmd.Id == inputId);
			if(cmdToDelete != null)
			{
				_context.RobotCommands.Remove(cmdToDelete);
				_context.SaveChanges();
				return 1;
			}
			else
			{
				return 0;
			}
        }

		public void Dispose()
		{
			((IDisposable)_context).Dispose();
			GC.SuppressFinalize(this);	
		}

		public List<RobotCommand> GetMoveCommands()
        {

			return [.. _context.RobotCommands.Where(cmd => cmd.IsMoveCommand == true)];
        }

        public RobotCommand? GetRobotCommandById(int inputId)
        {
			return _context.RobotCommands.FirstOrDefault(cmd => cmd.Id == inputId);
        }

        public RobotCommand? GetRobotCommandByName(string inputName)
        {
			return _context.RobotCommands.FirstOrDefault(cmd => cmd.Name == inputName);
        }

        public List<RobotCommand> GetRobotCommands()
        {
			return [.. _context.RobotCommands];
		}

        public RobotCommand? InsertRobotCommand(RobotCommand robotCommand)
        {
			RobotCommand newCommand = _context.RobotCommands.Add(robotCommand).Entity;
			_context.SaveChanges();
			return newCommand;
		}

        public int UpdateRobotCommand(RobotCommand robotCommand)
        {
			_context.RobotCommands.Update(robotCommand);
			_context.SaveChanges();
			return 0;
        }
    }
}
