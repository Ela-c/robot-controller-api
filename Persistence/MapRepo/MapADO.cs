using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Persistence;

namespace robot_controller_api.Persistance
{
	public class MapADO : IMapDataAccess
	{
        private readonly string CONNECTION_STRING = DbConfig.ConnectionString;
        public List<Map> GetAllMaps()
		{
			var maps = new List<Map>();
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.map", conn);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				int rows = (int)reader["rows"];
				int columns = (int)reader["columns"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				var map = new Map(id, name, description, rows, columns, rows == columns, createdDate, modifiedDate);
				maps.Add(map);
			}

			return maps;
		}

		public List<Map> GetSquareMaps()
		{
			var maps = new List<Map>();
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.map WHERE map.issquare = TRUE", conn);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				int rows = (int)reader["rows"];
				int columns = (int)reader["columns"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				var map = new Map(id, name, description, rows, columns, rows == columns, createdDate, modifiedDate);
				maps.Add(map);
			}
			return maps;
		}

		public Map? GetMapById(int inputId)
		{
			Map? map = null;

			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.map WHERE map.id = ($1)", conn)
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
				int rows = (int)reader["rows"];
				int columns = (int)reader["columns"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				map = new Map(id, name, description, rows, columns, rows == columns, createdDate, modifiedDate);
			}
			return map;
		}

		public Map? GetMapByName(string inputName)
		{
			Map? map = null;

			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("SELECT * FROM public.map WHERE map.name = ($1)", conn)
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
				int rows = (int)reader["rows"];
				int columns = (int)reader["columns"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				map = new Map(id, name, description, rows, columns, rows == columns, createdDate, modifiedDate);
			}
			return map;
		}

		public int UpdateMap(Map map)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("UPDATE public.map " +
				"SET " +
				"name = ($1), " +
				"description = ($2), " +
				"rows = ($3), " +
				"columns = ($4), " +
				"modified_date = ($5) " +
				"WHERE map.id = ($6)", conn)
			{
				Parameters =
				{
					new() { Value = map.Name},
					new() { Value = map.Description == null ? DBNull.Value : map.Description, DbType = System.Data.DbType.String, IsNullable = true},
					new() { Value = map.Rows},
					new() { Value = map.Columns},
					new() { Value = map.ModifiedDate},
					new() { Value = map.Id},
				}
			};

			int nRows = cmd.ExecuteNonQuery();
			return nRows;
		}

		public int DeleteMap(int inputId)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("DELETE FROM public.map WHERE map.id = ($1)", conn)
			{
				Parameters =
				{
					new() {Value = inputId},
				}
			};
			int nRows = cmd.ExecuteNonQuery();
			return nRows;
		}

		public Map? InsertMap(Map map)
		{
			using var conn = new NpgsqlConnection(CONNECTION_STRING);
			conn.Open();
			using var cmd = new NpgsqlCommand("INSERT INTO public.map (name, description, rows, columns, created_date, modified_date)" +
				"VALUES" +
				"(($1), " +
				"($2), " +
				"($3), " +
				"($4), " +
				"($5), " +
				"($6)) RETURNING *", conn)
			{
				Parameters =
				{
					new() {Value =map.Name},
					new() {Value = map.Description == null ? DBNull.Value : map.Description, DbType = System.Data.DbType.String, IsNullable = true},
					new() {Value =map.Rows},
					new() {Value =map.Columns},
					new() {Value =map.CreatedDate},
					new() {Value =map.ModifiedDate},
				}
			};

			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				int id = (int)reader["id"];
				string name = (string)reader["name"];
				string? description = reader["description"] is not System.DBNull ? (string)reader["description"] : null;
				int rows = (int)reader["rows"];
				int columns = (int)reader["columns"];
				var createdDate = (DateTime)reader["created_date"];
				var modifiedDate = (DateTime)reader["modified_date"];

				map = new Map(id, name, description, rows, columns, rows==columns, createdDate, modifiedDate);
			}
			return map;
		}
	}
}
