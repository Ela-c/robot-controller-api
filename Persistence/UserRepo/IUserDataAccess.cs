using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
	public interface IUserDataAccess
	{
		List<User> GetAllUsers();
		User? GetUserById(int inputId);
		List<User> GetAdminUsers();
		User? GetUserByEmail(string email);
		User? InsertUser(User user);
		bool UpdateUser(User user);
		bool DeleteUser(int inputId);
	}
}
