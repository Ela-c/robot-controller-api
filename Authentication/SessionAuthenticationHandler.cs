using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using robot_controller_api.Persistence;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace robot_controller_api.Authentication
{
    public class SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISessionDataAccess sessionDataAccess,
        SessionTokenService tokenService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private readonly ISessionDataAccess _sessionDataAccess = sessionDataAccess;
        private readonly SessionTokenService _tokenService = tokenService;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!Request.Cookies.TryGetValue("session_token", out var token) || string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication required"));
            }

            string tokenHash = _tokenService.HashToken(token);
            var activeSession = _sessionDataAccess.GetActiveSessionByTokenHash(tokenHash);
            if (activeSession == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired session"));
            }

            _sessionDataAccess.TouchSession(activeSession.Id);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, activeSession.UserId.ToString()),
                new Claim(ClaimTypes.Name, $"{activeSession.User.FirstName} {activeSession.User.LastName}"),
                new Claim(ClaimTypes.Role, activeSession.User.Role ?? ""),
                new Claim(ClaimTypes.Email, activeSession.User.Email),
                new Claim("description", activeSession.User.Description ?? "")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
