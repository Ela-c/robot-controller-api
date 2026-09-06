namespace robot_controller_api.Models
{
    public class UserSession
    {
        public UserSession()
        {
        }

        public UserSession(int id, int userId, string tokenHash, DateTime createdDate, DateTime expiresDate, DateTime? revokedDate, DateTime? lastSeenDate)
        {
            Id = id;
            UserId = userId;
            TokenHash = tokenHash;
            CreatedDate = createdDate;
            ExpiresDate = expiresDate;
            RevokedDate = revokedDate;
            LastSeenDate = lastSeenDate;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresDate { get; set; }
        public DateTime? RevokedDate { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public User User { get; set; } = null!;
    }
}
