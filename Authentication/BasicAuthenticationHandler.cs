using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text;
using robot_controller_api.Models;
using System.Security.Claims;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using robot_controller_api.Persistence;

namespace robot_controller_api.Authentication
{
	public class BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IUserDataAccess context) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
	{

		private readonly IUserDataAccess _context = context;

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			// Let anonymous users access unprotected routes
			var endpoint = Context.GetEndpoint();
			if(endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			Response.Headers.Append("WWW-Authenticate", @"Basic realm=""Access to the robot controller.""");
			
			var authHeader = Request.Headers.Authorization.ToString();

			if(string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("basic ", StringComparison.OrdinalIgnoreCase))
			{
				Response.StatusCode = 401;
				return Task.FromResult(AuthenticateResult.Fail("Authentication Failed"));
			}

			Log.Information("Request Authentication header {authHeader}", authHeader);

			string base64Text = authHeader.Split(' ')[1];
			byte[] base64Bytes = Convert.FromBase64String(base64Text);
			string plainText = Encoding.UTF8.GetString(base64Bytes);
			string[] credentials = plainText.Split(':');

			if(credentials.Length != 2)
			{
				Response.StatusCode = 401;
				return Task.FromResult(AuthenticateResult.Fail("Authentication Failed"));
			}

			string email = credentials[0];
			string password = credentials[1];

			Log.Information($"Decoded credentials Email: {email}, Password: {password}");

			// initialize context
			User? storedUser = _context.GetUserByEmail(email);
			
			// if user doesn't exists
			if(storedUser == null)
			{
				Log.Information("User with email {email} not found", email);
				Response.StatusCode = 401;
				return Task.FromResult(AuthenticateResult.Fail("Authentication Failed"));
			}

			// verify user's password
			bool pwVerificationResult = BCrypt.Net.BCrypt.EnhancedVerify(password, storedUser.PasswordHash);

			if (pwVerificationResult)
			{
				var basicClaims = new[]
				{
					new Claim("name", $"{storedUser.FirstName} {storedUser.LastName}"),
					new Claim(ClaimTypes.NameIdentifier, storedUser.Id.ToString()),
					new Claim(ClaimTypes.Role, storedUser.Role ?? ""),
				};

				var identity1 = new ClaimsIdentity(basicClaims, "Basic");

				var moreClaims = new[]
				{
					new Claim(ClaimTypes.Email, storedUser.Email),
				};

				var identity2 = new ClaimsIdentity(moreClaims, "Passport");

				var claimsPrincipal = new ClaimsPrincipal(new[] { identity1, identity2 });
				var authTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

				return Task.FromResult(AuthenticateResult.Success(authTicket));
			}
			Log.Information("User password didn't match");
			Response.StatusCode = 401;
			return Task.FromResult(AuthenticateResult.Fail("Authentication Failed"));
		}
	}
}
