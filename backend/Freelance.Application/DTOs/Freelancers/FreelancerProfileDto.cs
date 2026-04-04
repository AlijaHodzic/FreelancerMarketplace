namespace Freelance.Application.DTOs.Freelancers;

public class FreelancerProfileDto : FreelancerSummaryDto
{
    public string About { get; set; } = string.Empty;
    public List<FreelancerPortfolioItemDto> Portfolio { get; set; } = new();
    public List<FreelancerReviewDto> Testimonials { get; set; } = new();
}
