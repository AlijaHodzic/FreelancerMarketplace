using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Bids;

public class BidResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid FreelancerId { get; set; }

    public decimal Amount { get; set; }
    public string Message { get; set; } = string.Empty;

    public BidStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
