namespace Freelance.Application.DTOs.Freelancers;

public class UpdateFreelancerProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public string SuccessRate { get; set; } = string.Empty;
    public int CompletedProjects { get; set; }
    public decimal HourlyRate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<FreelancerPortfolioItemDto> Portfolio { get; set; } = new();
}
