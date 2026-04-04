namespace Freelance.Application.DTOs.Freelancers;

public class FreelancerSummaryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int Reviews { get; set; }
    public decimal HourlyRate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string ResponseTime { get; set; } = string.Empty;
    public string SuccessRate { get; set; } = string.Empty;
    public int CompletedProjects { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
}
