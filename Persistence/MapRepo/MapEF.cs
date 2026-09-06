using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public class MapEF(RobotContext context) : IMapDataAccess, IDisposable
    {
        private readonly RobotContext _robotContext = context;

        public int DeleteMap(int inputId)
        {
            var mapToDelete = _robotContext.Maps.FirstOrDefault(map => map.Id == inputId);
            if (mapToDelete != null)
            {
                _robotContext.Maps.Remove(mapToDelete);
                _robotContext.SaveChanges();
                return 1;
            }
            else
            {
                return 0;
            }
        }

		public void Dispose()
		{
			((IDisposable)_robotContext).Dispose();
			GC.SuppressFinalize(this);
		}

		public List<Map> GetAllMaps()
        {
            return [.. _robotContext.Maps];
        }

        public Map? GetMapById(int inputId)
        {
            return _robotContext.Maps.FirstOrDefault(map => map.Id == inputId);
        }

        public Map? GetMapByName(string inputName)
        {
            return _robotContext.Maps.FirstOrDefault(map => map.Name == inputName);
        }

        public List<Map> GetSquareMaps()
        {
            return [.. _robotContext.Maps.Where(map => map.IsSquare == true)];
        }

        public Map? InsertMap(Map map)
        {
            Map newMap = _robotContext.Maps.Add(map).Entity;
            _robotContext.SaveChanges();
            return newMap;
        }

        public int UpdateMap(Map map)
        {
            _robotContext.Maps.Update(map);
            _robotContext.SaveChanges();
            return 0;
        }
    }
}
