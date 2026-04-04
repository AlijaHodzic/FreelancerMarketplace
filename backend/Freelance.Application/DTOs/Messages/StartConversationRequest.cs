namespace Freelance.Application.DTOs.Messages;

public class StartConversationRequest
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string InitialMessage { get; set; } = string.Empty;
}
