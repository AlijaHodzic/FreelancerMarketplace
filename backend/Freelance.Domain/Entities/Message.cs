using Freelance.Domain.Common;

namespace Freelance.Domain.Entities
{
    public class Message : AuditableEntity
    {
        public Guid ConversationId { get; set; }
        public Conversation Conversation { get; set; } = null!;

        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
    }
}
