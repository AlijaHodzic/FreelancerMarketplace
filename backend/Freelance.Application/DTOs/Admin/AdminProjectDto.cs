using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Admin;

public class AdminProjectDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public int BidsCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
