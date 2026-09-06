using Npgsql;
using robot_controller_api.Models;
namespace robot_controller_api.Persistence
{
    public class RobotCommandRepository: IRobotCommandDataAccess, IRepository
	{
		private IRepository _repo => this;

		public List<RobotCommand> GetRobotCommands()
		{
			return _repo.ExecuteReader<RobotCommand>("SELECT * FROM public.robot_command");
		}

		public int UpdateRobotCommand(RobotCommand updatedCommand)
		{
			var sqlParams = new NpgsqlParameter[]{
				new("id", updatedCommand.Id),
				new("name", updatedCommand.Name),
				new("description", updatedCommand.Description ?? (object)DBNull.Value),
				new("is_move_command", updatedCommand.IsMoveCommand)
			};
			return _repo.ExecuteNonQuery("UPDATE robot_command SET name=@name, description=@description, is_move_command = @is_move_command, modified_date = current_timestamp WHERE id = @id", sqlParams);
		}

		public int DeleteRobotCommand(int inputId)
		{
			var sqlParams = new NpgsqlParameter[]{
				new("id", inputId)
			};
			return _repo.ExecuteNonQuery("DELETE FROM public.robot_command WHERE id = @id", sqlParams);
		}

		public List<RobotCommand> GetMoveCommands()
		{
			return _repo.ExecuteReader<RobotCommand>("SELECT * FROM public.robot_command WHERE robot_command.is_move_command = TRUE");
		}

		public RobotCommand? GetRobotCommandById(int inputId)
		{
			var sqlParams = new NpgsqlParameter[]
			{
				new("id", inputId),
			};

			return _repo.ExecuteReader<RobotCommand>("SELECT * FROM public.robot_command WHERE robot_command.id = (@id)", sqlParams).FirstOrDefault();
		}

		public RobotCommand? GetRobotCommandByName(string inputName)
		{
			var sqlParams = new NpgsqlParameter[]
			{
				new("name", inputName),
			};

			return _repo.ExecuteReader<RobotCommand>("SELECT * FROM public.robot_command WHERE robot_command.name = (@name)", sqlParams).FirstOrDefault();
		}

		public RobotCommand? InsertRobotCommand(RobotCommand robotCommand)
		{
			var sqlParams = new NpgsqlParameter[]{
				new("name", robotCommand.Name),
				new("description", robotCommand.Description ?? (object)DBNull.Value),
				new("is_move_command", robotCommand.IsMoveCommand),
				new("created_date", robotCommand.CreatedDate),
				new("modified_date", robotCommand.ModifiedDate)
			};
			return _repo.ExecuteReader<RobotCommand>("INSERT INTO public.robot_command (name, description, is_move_command, created_date, modified_date)" +
				"VALUES" +
				"((@name), " +
				"(@description), " +
				"(@is_move_command), " +
				"(@created_date), " +
				"(@modified_date)) RETURNING *", sqlParams).Single();
		}
	}
}
