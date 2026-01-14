using Freelance.Domain.Common;
using Freelance.Domain.Enums;

namespace Freelance.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Client;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
