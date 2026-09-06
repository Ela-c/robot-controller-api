using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence
{
    public class SessionEF(RobotContext robotContext) : ISessionDataAccess, IDisposable
    {
        private readonly RobotContext _context = robotContext;

        public UserSession CreateSession(UserSession session)
        {
            var createdSession = _context.UserSessions.Add(session).Entity;
            _context.SaveChanges();
            return createdSession;
        }

        public UserSession? GetActiveSessionByTokenHash(string tokenHash)
        {
            var now = DateTime.Now;

            return _context.UserSessions
                .Include(session => session.User)
                .FirstOrDefault(session =>
                    session.TokenHash == tokenHash &&
                    session.RevokedDate == null &&
                    session.ExpiresDate > now);
        }

        public bool RevokeSessionByTokenHash(string tokenHash)
        {
            var session = _context.UserSessions.FirstOrDefault(existingSession => existingSession.TokenHash == tokenHash);
            if (session == null)
            {
                return false;
            }

            session.RevokedDate = DateTime.Now;
            session.LastSeenDate = DateTime.Now;
            _context.UserSessions.Update(session);
            _context.SaveChanges();
            return true;
        }

        public int RevokeAllActiveSessionsByUserId(int userId)
        {
            var now = DateTime.Now;
            var activeSessions = _context.UserSessions
                .Where(existingSession =>
                    existingSession.UserId == userId &&
                    existingSession.RevokedDate == null &&
                    existingSession.ExpiresDate > now)
                .ToList();

            if (activeSessions.Count == 0)
            {
                return 0;
            }

            foreach (var session in activeSessions)
            {
                session.RevokedDate = now;
                session.LastSeenDate = now;
            }

            _context.UserSessions.UpdateRange(activeSessions);
            _context.SaveChanges();
            return activeSessions.Count;
        }

        public bool TouchSession(int sessionId)
        {
            var session = _context.UserSessions.FirstOrDefault(existingSession => existingSession.Id == sessionId);
            if (session == null)
            {
                return false;
            }

            session.LastSeenDate = DateTime.Now;
            _context.UserSessions.Update(session);
            _context.SaveChanges();
            return true;
        }

        public void Dispose()
        {
            ((IDisposable)_context).Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
