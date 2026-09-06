using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Persistance
{
	public class RobotCommandADO : IRobotCommandDataAccess
	{
		private readonly string CONNECTION_STRING = DbConfig.ConnectionString;
        public List<RobotCommand> GetRobotCommands()
		{
			var robotCommands = new List<RobotCommand>();
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM robot_command", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				bool isMoveCommand = (bool)reader["is_move_command"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				var robotCommand = new RobotCommand(id, name, description, isMoveCommand, createdDate, modifiedDate);
				robotCommands.Add(robotCommand);
			}

			return robotCommands;
		}

		public List<RobotCommand> GetMoveCommands()
		{
			var robotCommands = new List<RobotCommand>();
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.robot_command WHERE robot_command.is_move_command = TRUE", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				bool isMoveCommand = (bool)reader["is_move_command"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				var robotCommand = new RobotCommand(id, name, description, isMoveCommand, createdDate, modifiedDate);
				robotCommands.Add(robotCommand);
			}
			return robotCommands;
		}

		public RobotCommand? GetRobotCommandById(int inputId)
		{
			RobotCommand? robotCommand = null;

			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.robot_command WHERE robot_command.id = ($1)", conn)
			{
				Parameters =
				{
					new() { Value = inputId }
				}
			};
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				bool isMoveCommand = (bool)reader["is_move_command"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				robotCommand = new RobotCommand(id, name, description, isMoveCommand, createdDate, modifiedDate);
			}
			return robotCommand;
		}

		public RobotCommand? GetRobotCommandByName(string inputName)
		{
			RobotCommand? robotCommand = null;

			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.robot_command WHERE robot_command.name = ($1)", conn)
			{
				Parameters =
				{
					new() { Value = inputName }
				}
			};
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				bool isMoveCommand = (bool)reader["is_move_command"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				robotCommand = new RobotCommand(id, name, description, isMoveCommand, createdDate, modifiedDate);
			}
			return robotCommand;
		}

		public int UpdateRobotCommand(RobotCommand robotCommand)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("UPDATE public.robot_command " +
				"SET " +
				"name = ($1), " +
				"description = ($2), " +
				"is_move_command = ($3), " +
				"modified_date = ($4) " +
				"WHERE robot_command.id = ($5)", conn)
			{
				Parameters =
				{
					new() { Value = robotCommand.Name},
					new() { Value = robotCommand.Description == null ? DBNull.Value : robotCommand.Description, DbType = System.Data.DbType.String, IsNullable = true},
					new() { Value = robotCommand.IsMoveCommand},
					new() { Value = robotCommand.ModifiedDate},
					new() { Value = robotCommand.Id}
				}
			};

			int nRows = cmd.ExecuteNonQuery();
			return nRows;
		}

		public int DeleteRobotCommand(int inputId)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("DELETE FROM public.robot_command WHERE robot_command.id = ($1)", conn)
			{
				Parameters =
				{
					new() {Value = inputId},
				}
			};
			int nRows = cmd.ExecuteNonQuery();
			return nRows;
		}

		public RobotCommand? InsertRobotCommand(RobotCommand robotCommand)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("INSERT INTO public.robot_command (name, description, is_move_command, created_date, modified_date)" +
				"VALUES" +
				"(($1), " +
				"($2), " +
				"($3), " +
				"($4), " +
				"($5)) RETURNING *", conn)
			{
				Parameters =
				{
					new() {Value =robotCommand.Name},
					new() {Value = robotCommand.Description == null ? DBNull.Value : robotCommand.Description, DbType = System.Data.DbType.String, IsNullable = true},
					new() {Value =robotCommand.IsMoveCommand},
					new() {Value =robotCommand.CreatedDate},
					new() {Value =robotCommand.ModifiedDate},
				}
			};

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				bool isMoveCommand = (bool)reader["is_move_command"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				robotCommand = new RobotCommand(id, name, description, isMoveCommand, createdDate, modifiedDate);
			}
			return robotCommand;
		}
	}
}
