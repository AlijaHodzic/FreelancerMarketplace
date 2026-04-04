using Freelance.Application.DTOs.Messages;

namespace Freelance.Application.Interfaces;

public interface IMessagingService
{
    Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(Guid userId);
    Task<ConversationThreadDto> GetConversationAsync(Guid userId, Guid conversationId);
    Task<ConversationThreadDto> StartConversationAsync(Guid senderId, StartConversationRequest request);
    Task<MessageDto> SendMessageAsync(Guid senderId, Guid conversationId, SendMessageRequest request);
}
