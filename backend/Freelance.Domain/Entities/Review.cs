using Freelance.Domain.Common;

namespace Freelance.Domain.Entities;

public class Review : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;

    public Guid FreelancerId { get; set; }
    public User Freelancer { get; set; } = null!;

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
