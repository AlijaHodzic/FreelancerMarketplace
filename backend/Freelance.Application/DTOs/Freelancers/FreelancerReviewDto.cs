namespace Freelance.Application.DTOs.Freelancers;

public class FreelancerReviewDto
{
    public string Author { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
}
