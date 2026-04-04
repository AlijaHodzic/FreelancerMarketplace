using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Projects
{
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public ProjectStatus Status { get; set; }
        public Guid? AssignedFreelancerId { get; set; }
        public string AssignedFreelancerName { get; set; } = string.Empty;
        public string AssignedFreelancerEmail { get; set; } = string.Empty;
        public DateTime? HiredAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public bool CanReview { get; set; }
        public bool HasReview { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
