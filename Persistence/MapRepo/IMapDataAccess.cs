using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public interface IMapDataAccess
    {
        int DeleteMap(int inputId);
        List<Map> GetAllMaps();
        Map? GetMapById(int inputId);
        Map? GetMapByName(string inputName);
        List<Map> GetSquareMaps();
        Map? InsertMap(Map map);
        int UpdateMap(Map map);
    }
}