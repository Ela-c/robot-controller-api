using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public class MapRepository : IMapDataAccess, IRepository
    {
        private IRepository _repo => this;
        public int DeleteMap(int inputId)
        {
			var sqlParams = new NpgsqlParameter[]{
				new("id", inputId)
			};
			return _repo.ExecuteNonQuery("DELETE FROM public.map WHERE map.id = (@id)", sqlParams);
        }

        public List<Map> GetAllMaps()
        {
			return _repo.ExecuteReader<Map>("SELECT * FROM public.map");
		}

        public Map? GetMapById(int inputId)
        {
			var sqlParams = new NpgsqlParameter[]
			{
				new("id", inputId)
			};

			return _repo.ExecuteReader<Map>("SELECT * FROM public.map WHERE map.id = (@id)", sqlParams).FirstOrDefault();
		}

        public Map? GetMapByName(string inputName)
        {
			var sqlParams = new NpgsqlParameter[]
			{
				new("name", inputName)
			};

			 return _repo.ExecuteReader<Map>("SELECT * FROM public.map WHERE map.name = (@name)", sqlParams).FirstOrDefault();
        }

        public List<Map> GetSquareMaps()
        {
			return _repo.ExecuteReader<Map>("SELECT * FROM public.map WHERE map.is_square = TRUE");
        }

        public Map? InsertMap(Map map)
        {
			var sqlParams = new NpgsqlParameter[]
			{
				new("name", map.Name),
				new("description", map.Description ?? (object)DBNull.Value),
				new("rows", map.Rows),
				new("columns", map.Columns),
				new("created_date", map.CreatedDate),
				new("modified_date", map.ModifiedDate),
			};

			return _repo.ExecuteReader<Map>("INSERT INTO public.map (name, description, rows, columns, created_date, modified_date)" +
				"VALUES" +
				"((@name), " +
				"(@description), " +
				"(@rows), " +
				"(@columns), " +
				"(@created_date), " +
				"(@modified_date)) RETURNING *", sqlParams).Single();
		}

        public int UpdateMap(Map map)
        {
			var sqlParams = new NpgsqlParameter[]{
				new("id", map.Id),
				new("name", map.Name),
				new("description", map.Description ?? (object)DBNull.Value),
				new("rows", map.Rows),
				new("columns", map.Columns),
				new("modified_date", map.ModifiedDate),
			};

			return _repo.ExecuteNonQuery("UPDATE public.map " +
				"SET " +
				"name = (@name), " +
				"description = (@description), " +
				"rows = (@rows), " +
				"columns = (@columns), " +
				"modified_date = (@modified_date) " +
				"WHERE map.id = (@id)", sqlParams);
		}
    }
}
