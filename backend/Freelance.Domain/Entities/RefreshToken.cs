using Freelance.Domain.Common;

namespace Freelance.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }

        public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
    }
}
