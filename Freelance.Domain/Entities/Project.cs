using Freelance.Domain.Common;
using Freelance.Domain.Enums;

namespace Freelance.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid ClientId { get; set; }
        public User Client { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Open;

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}
