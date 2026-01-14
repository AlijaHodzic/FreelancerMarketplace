using Freelance.Domain.Common;
using Freelance.Domain.Enums;

namespace Freelance.Domain.Entities
{
    public class Bid : AuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid FreelancerId { get; set; }
        public User Freelancer { get; set; } = null!;

        public decimal Amount { get; set; }
        public string Message { get; set; } = string.Empty;

        public BidStatus Status { get; set; } = BidStatus.Pending;
    }
}
