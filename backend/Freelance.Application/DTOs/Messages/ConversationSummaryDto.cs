using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Messages;

public class ConversationSummaryDto
{
    public Guid Id { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public UserRole OtherUserRole { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string LastMessagePreview { get; set; } = string.Empty;
    public DateTime LastMessageAtUtc { get; set; }
}
