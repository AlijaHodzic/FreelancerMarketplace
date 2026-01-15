using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Projects
{
    public class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BudgetMin { get; set; }
        public decimal BudgetMax { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
