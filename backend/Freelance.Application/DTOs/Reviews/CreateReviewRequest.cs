namespace Freelance.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    public Guid ProjectId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
