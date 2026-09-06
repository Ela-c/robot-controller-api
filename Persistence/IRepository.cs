using Npgsql;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace robot_controller_api.Persistence
{
	public interface IRepository
	{
		private static string GetConnectionString()
		{
			// Build a minimal IConfiguration to read connection string from appsettings.json or environment variables.
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddEnvironmentVariables();
			var config = builder.Build();
			var conn = config.GetConnectionString("RobotConnection")
					   ?? Environment.GetEnvironmentVariable("ConnectionStrings__RobotConnection")
					   ?? Environment.GetEnvironmentVariable("RobotConnection")
					   ?? string.Empty;

			return conn;
		}

		public List<T> ExecuteReader<T>(string sqlCommand, NpgsqlParameter[]? dbParams = null) where T : class, new()
		{
			var entities = new List<T>();
			using var conn = new NpgsqlConnection(GetConnectionString());
			conn.Open();
			using var cmd = new NpgsqlCommand(sqlCommand, conn);
			// Some of our SQL commands might have SQL parameters we will need to pass to DB.MS
			if (dbParams is not null)
			{
				// CommandType is unnecessary for PostgreSQL but can be used in other DB engines like Oracle or SQL Server. MS
				//cmd.CommandType = CommandType.Text;
				cmd.Parameters.AddRange(dbParams.Where(x => x.Value is not null).ToArray());
			}
			using var dr = cmd.ExecuteReader();
			while (dr.Read())
			{
				var entity = new T();
				dr.MapTo(entity);
				entities.Add(entity);
			}
			return entities;
		}

		public int ExecuteNonQuery(string sqlCommand, NpgsqlParameter[]? dbParams = null)
		{
			int rowsAffected;
			using var conn = new NpgsqlConnection(GetConnectionString());
			conn.Open();
			using var cmd = new NpgsqlCommand(sqlCommand, conn);

			if (dbParams is not null)
			{
				cmd.Parameters.AddRange(dbParams.Where(x => x.Value is not null).ToArray());
			}

			rowsAffected = cmd.ExecuteNonQuery();

			return rowsAffected;
		}
	}
}
