using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Bids;

public class BidResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FreelancerId { get; set; }
    public string FreelancerName { get; set; } = string.Empty;
    public string FreelancerEmail { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;

    public string ProjectTitle { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public decimal ProjectBudgetMin { get; set; }
    public decimal ProjectBudgetMax { get; set; }

    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;

    public BidStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
