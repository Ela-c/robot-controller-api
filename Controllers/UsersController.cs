using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Authentication;
using robot_controller_api.Models;
using robot_controller_api.Persistence;
using Serilog;
using System.Security.Claims;

namespace robot_controller_api.Controllers
{
	[ApiController]
	[Route("users")]
	public class UserController: ControllerBase
	{

		private readonly IUserDataAccess _repo;
		private readonly ISessionDataAccess _sessionRepo;
		private readonly SessionTokenService _sessionTokenService;

		public UserController(IUserDataAccess repo, ISessionDataAccess sessionRepo, SessionTokenService sessionTokenService)
		{
			_repo = repo;
			_sessionRepo = sessionRepo;
			_sessionTokenService = sessionTokenService;
		}
		
		[Authorize(Policy = "UserAdmin")]
		[HttpGet]
		public IEnumerable<User> GetAllUsers()
		{
			return _repo.GetAllUsers();
		}

		[Authorize(Policy = "UserAdmin")]
		[HttpGet("admin")]
		public IEnumerable<User> GetAdminUsers() 
		{ 
			return _repo.GetAdminUsers();
		}

		[Authorize(Policy = "UserAdmin")]
		[HttpGet("{id:int}", Name = "GetUserById")]
		public IActionResult GetUserById(int id) 
		{ 
			var user = _repo.GetUserById(id);
			if( user != null)
			{
				return Ok(user);
			}

			return NotFound("User not found");
		}

		[AllowAnonymous]
		[HttpPost]
		public IActionResult AddUser(User user)
		{
			if (user == null)
			{
				return BadRequest("Empty user");
			}

			// check for user uniqueness 
			if(_repo.GetUserByEmail(user.Email) != null)
			{
				return BadRequest("Email already exist");
			}

			User newUser = new();
			User? createdUser;
			try
			{
				newUser.FirstName = user.FirstName;
				newUser.LastName = user.LastName;
				newUser.Email = user.Email;
				newUser.Description = user.Description;
				newUser.Role = user.Role?.ToLower();

				//var hasher = new PasswordHasher<User>();
				//var pwHash = hasher.HashPassword(user, user.PasswordHash);
				var pwHash = BCrypt.Net.BCrypt.EnhancedHashPassword(user.PasswordHash);
				newUser.PasswordHash = pwHash;

				var date = DateTime.Now;
				newUser.CreatedDate = date;
				newUser.ModifiedDate = date;
				createdUser = _repo.InsertUser(newUser);
			}
			catch(Exception ex)
			{
				Log.Information("(add user, action) Error: {ex}", ex);
				return BadRequest();
			}

			if( createdUser == null )
			{
				Log.Information("(add user, action) Error: no user was updated");
				return BadRequest();
			}
			return CreatedAtAction("GetUserById", new { id = createdUser.Id },createdUser);

		}

		[AllowAnonymous]
		[HttpPost("login")]
		public IActionResult Login(LoginModel loginInfo)
		{
			if (loginInfo == null || string.IsNullOrWhiteSpace(loginInfo.Email) || string.IsNullOrWhiteSpace(loginInfo.Password))
			{
				return BadRequest("Invalid login payload");
			}

			var storedUser = _repo.GetUserByEmail(loginInfo.Email);
			if (storedUser == null)
			{
				return Unauthorized();
			}

			if (!BCrypt.Net.BCrypt.EnhancedVerify(loginInfo.Password, storedUser.PasswordHash))
			{
				return Unauthorized();
			}

			var now = DateTime.Now;
			var sessionToken = _sessionTokenService.GenerateToken();
			var sessionTokenHash = _sessionTokenService.HashToken(sessionToken);

			var createdSession = _sessionRepo.CreateSession(new UserSession
			{
				UserId = storedUser.Id,
				TokenHash = sessionTokenHash,
				CreatedDate = now,
				LastSeenDate = now,
				ExpiresDate = now.AddDays(7)
			});

			Response.Cookies.Append("session_token", sessionToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = createdSession.ExpiresDate,
				IsEssential = true
			});

			return Ok(new { message = "Login successful" });
		}

		[Authorize(Policy = "CatalogRead")]
		[HttpPost("logout")]
		public IActionResult Logout()
		{
			if (Request.Cookies.TryGetValue("session_token", out var sessionToken) && !string.IsNullOrWhiteSpace(sessionToken))
			{
				var sessionTokenHash = _sessionTokenService.HashToken(sessionToken);
				_sessionRepo.RevokeSessionByTokenHash(sessionTokenHash);
			}

			Response.Cookies.Delete("session_token", new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				IsEssential = true
			});

			return NoContent();
		}

		[Authorize(Policy = "CatalogRead")]
		[HttpPost("logout-from-all-devices")]
		public IActionResult LogoutFromAllDevices()
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdClaim, out var userId))
			{
				return Unauthorized();
			}

			_sessionRepo.RevokeAllActiveSessionsByUserId(userId);

			Response.Cookies.Delete("session_token", new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				IsEssential = true
			});

			return NoContent();
		}

		[Authorize(Policy = "UserAdmin")]
		[HttpPut("{id:int}")]
		public IActionResult UpdateUser(int id, User user) 
		{ 
			if(user == null)
			{
				Log.Information("(updated user, action) Error: user parameter was null");
				return BadRequest("No user provided");
			}

			//find user
			var previousUser = _repo.GetUserById(id);
			if(previousUser == null )
			{
				Log.Information("(update user, action) Error: user id not found => {id}", id);
				return NotFound($"No users with id {id}");
			}

			//update user
			try
			{
				previousUser.FirstName = user.FirstName;
				previousUser.LastName = user.LastName;
				previousUser.Role = user.Role?.ToLower();
				previousUser.Description = user.Description;
				previousUser.ModifiedDate = DateTime.Now;
				_repo.UpdateUser(previousUser);
			}
			catch (Exception ex)
			{
				Log.Information("(update user, action) Error: {ex}", ex);
				return BadRequest();
			}

			return NoContent();	
		}

		[Authorize(Policy = "UserAdmin")]
		[HttpDelete("{id:int}")]
		public IActionResult DeleteUser(int id)
		{
			if (_repo.DeleteUser(id))
			{
				return NoContent();
			}
			return NotFound();
		}

		[Authorize(Policy = "SelfOrAdmin")]
		[HttpPatch("{id:int}")]
		public IActionResult PatchUserEmailAndPassword(int id, LoginModel newLoginInfo) 
		{
			if (newLoginInfo == null)
			{
				Log.Information("(patch user, action) Error: newLoginInfo parameter was null");
				return BadRequest("No data provided");
			}

			//find user
			var existingUser = _repo.GetUserById(id);
			if (existingUser == null)
			{
				Log.Information("(patch user, action) Error: user id not found => {id}", id);
				return NotFound($"No users with id {id}");
			}

			if(newLoginInfo.Email != existingUser.Email)
			{
				// check if new email already exists
				if (_repo.GetUserByEmail(newLoginInfo.Email) != null)
				{
					return BadRequest("New email already exists");
				}
			}

			// hash new password
			//var hasher = new PasswordHasher<User>();
			//var pwHash = hasher.HashPassword(existingUser, newLoginInfo.Password);

			var pwHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newLoginInfo.Password);

			// update only email and pass of user
			try
			{
				existingUser.Email = newLoginInfo.Email;
				existingUser.PasswordHash = pwHash;
				existingUser.ModifiedDate = DateTime.Now;
				_repo.UpdateUser(existingUser);
			}
			catch (Exception error)
			{
				Log.Information("(patch user, action) Error: {error}", error);
				return BadRequest();
			}

			return NoContent();
		}
	}
}
