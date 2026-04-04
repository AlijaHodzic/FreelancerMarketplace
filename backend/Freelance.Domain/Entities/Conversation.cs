using Freelance.Domain.Common;

namespace Freelance.Domain.Entities
{
    public class Conversation : AuditableEntity
    {
        public Guid ClientId { get; set; }
        public User Client { get; set; } = null!;

        public Guid FreelancerId { get; set; }
        public User Freelancer { get; set; } = null!;

        public string Subject { get; set; } = string.Empty;
        public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
