using Freelance.Domain.Common;
using Freelance.Domain.Enums;

namespace Freelance.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public Guid? AssignedFreelancerId { get; set; }
        public User? AssignedFreelancer { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public DateTime? HiredAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Open;

        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
