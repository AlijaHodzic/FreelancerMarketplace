namespace Freelance.Application.DTOs.Projects;

public class HireFreelancerRequest
{
    public Guid FreelancerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
}
