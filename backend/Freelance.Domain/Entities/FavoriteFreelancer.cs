using Freelance.Domain.Common;

namespace Freelance.Domain.Entities;

public class FavoriteFreelancer : AuditableEntity
{
    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;

    public Guid FreelancerId { get; set; }
    public User Freelancer { get; set; } = null!;
}
