using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public interface ISessionDataAccess
    {
        UserSession CreateSession(UserSession session);
        UserSession? GetActiveSessionByTokenHash(string tokenHash);
        bool RevokeSessionByTokenHash(string tokenHash);
        int RevokeAllActiveSessionsByUserId(int userId);
        bool TouchSession(int sessionId);
    }
}
