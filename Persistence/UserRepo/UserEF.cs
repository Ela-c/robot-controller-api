using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
	public class UserEF(RobotContext robotContext): IUserDataAccess, IDisposable
	{
		private readonly RobotContext _context = robotContext;

		public bool DeleteUser(int inputId)
		{
			// find user
			User? userToDelete = _context.Users.FirstOrDefault(user => user.Id == inputId);
			if (userToDelete != null)
			{
				// remove user
				_context.Users.Remove(userToDelete);
				_context.SaveChanges();
				return true;
			}
			return false;
		}

		public void Dispose()
		{
			((IDisposable)_context).Dispose();
			GC.SuppressFinalize(this);
		}

		public List<User> GetAdminUsers()
		{
			var adminUsers = _context.Users.Where(user => user.Role == "admin");
			return [..adminUsers];
		}

		public List<User> GetAllUsers()
		{
			var users = _context.Users;
			return [..users];
		}

		public User? GetUserByEmail(string email)
		{
			return _context.Users.FirstOrDefault(User => User.Email == email);
		}

		public User? GetUserById(int inputId)
		{
			var storedUser = _context.Users.FirstOrDefault(user => user.Id == inputId);
			return storedUser;
		}

		public User? InsertUser(User user)
		{
			var storedUser = _context.Users.Add(user).Entity;
			_context.SaveChanges();
			return storedUser;
		}

		public bool UpdateUser(User user)
		{
			_context.Users.Update(user);
			_context.SaveChanges();
			return true;
		}
	}
}
