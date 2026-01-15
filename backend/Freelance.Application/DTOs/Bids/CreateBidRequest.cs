using System.ComponentModel.DataAnnotations;

namespace Freelance.Application.DTOs.Bids;

public class CreateBidRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Range(1, 1_000_000)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
