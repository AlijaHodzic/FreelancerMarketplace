using Freelance.Domain.Enums;

namespace Freelance.Application.DTOs.Messages;

public class ConversationThreadDto
{
    public Guid Id { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public UserRole OtherUserRole { get; set; }
    public string Subject { get; set; } = string.Empty;
    public List<MessageDto> Messages { get; set; } = new();
}
