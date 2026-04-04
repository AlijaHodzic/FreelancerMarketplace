namespace Freelance.Application.DTOs.Freelancers;

public class FreelancerPortfolioItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
