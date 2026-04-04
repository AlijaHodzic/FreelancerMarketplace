using Freelance.Application.DTOs.Messages;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Domain.Enums;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notificationService;

        public MessagingService(AppDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ConversationSummaryDto>> GetConversationsAsync(Guid userId)
        {
            var conversations = await _db.Conversations
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.Messages)
                .Where(c => c.ClientId == userId || c.FreelancerId == userId)
                .OrderByDescending(c => c.LastMessageAtUtc)
                .ToListAsync();

            return conversations.Select(conversation =>
            {
                var otherUser = conversation.ClientId == userId ? conversation.Freelancer : conversation.Client;
                var lastMessage = conversation.Messages.OrderByDescending(message => message.CreatedAtUtc).FirstOrDefault();

                return new ConversationSummaryDto
                {
                    Id = conversation.Id,
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName,
                    OtherUserRole = otherUser.Role,
                    Subject = conversation.Subject,
                    LastMessagePreview = lastMessage?.Content ?? string.Empty,
                    LastMessageAtUtc = conversation.LastMessageAtUtc,
                };
            });
        }

        public async Task<ConversationThreadDto> GetConversationAsync(Guid userId, Guid conversationId)
        {
            var conversation = await _db.Conversations
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.Messages)
                    .ThenInclude(message => message.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new InvalidOperationException("Conversation not found");

            if (conversation.ClientId != userId && conversation.FreelancerId != userId)
                throw new InvalidOperationException("Access denied");

            return MapThread(userId, conversation);
        }

        public async Task<ConversationThreadDto> StartConversationAsync(Guid senderId, StartConversationRequest request)
        {
            var sender = await _db.Users.FirstOrDefaultAsync(user => user.Id == senderId);
            if (sender == null)
                throw new InvalidOperationException("Sender not found");

            var normalizedEmail = request.RecipientEmail.Trim().ToLowerInvariant();
            var recipient = await _db.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);
            if (recipient == null)
                throw new InvalidOperationException("Recipient not found");

            if (recipient.Id == sender.Id)
                throw new InvalidOperationException("You cannot message yourself");

            if (sender.Role == recipient.Role)
                throw new InvalidOperationException("Messaging is available only between clients and freelancers");

            var clientId = sender.Role == UserRole.Client ? sender.Id : recipient.Id;
            var freelancerId = sender.Role == UserRole.Freelancer ? sender.Id : recipient.Id;

            var conversation = await _db.Conversations
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .Include(c => c.Messages)
                    .ThenInclude(message => message.Sender)
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.FreelancerId == freelancerId);

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    ClientId = clientId,
                    FreelancerId = freelancerId,
                    Subject = string.IsNullOrWhiteSpace(request.Subject) ? $"Conversation with {recipient.FullName}" : request.Subject.Trim(),
                    LastMessageAtUtc = DateTime.UtcNow,
                };

                _db.Conversations.Add(conversation);
                await _db.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(request.InitialMessage))
            {
                await SendMessageAsync(senderId, conversation.Id, new SendMessageRequest
                {
                    Content = request.InitialMessage,
                });

                conversation = await _db.Conversations
                    .Include(c => c.Client)
                    .Include(c => c.Freelancer)
                    .Include(c => c.Messages)
                        .ThenInclude(message => message.Sender)
                    .FirstAsync(c => c.Id == conversation.Id);
            }

            return MapThread(senderId, conversation);
        }

        public async Task<MessageDto> SendMessageAsync(Guid senderId, Guid conversationId, SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                throw new InvalidOperationException("Message content is required");

            var conversation = await _db.Conversations
                .Include(c => c.Client)
                .Include(c => c.Freelancer)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new InvalidOperationException("Conversation not found");

            if (conversation.ClientId != senderId && conversation.FreelancerId != senderId)
                throw new InvalidOperationException("Access denied");

            var sender = await _db.Users.FirstOrDefaultAsync(user => user.Id == senderId);
            if (sender == null)
                throw new InvalidOperationException("Sender not found");

            var message = new Message
            {
                ConversationId = conversation.Id,
                SenderId = senderId,
                Content = request.Content.Trim(),
            };

            conversation.LastMessageAtUtc = DateTime.UtcNow;
            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            var recipient = conversation.ClientId == senderId ? conversation.Freelancer : conversation.Client;
            await _notificationService.CreateAsync(
                recipient.Id,
                "message",
                "New message",
                $"{sender.FullName} sent you a new message in \"{conversation.Subject}\".",
                $"/messages?conversationId={conversation.Id}");

            return new MessageDto
            {
                Id = message.Id,
                SenderId = sender.Id,
                SenderName = sender.FullName,
                Content = message.Content,
                CreatedAtUtc = message.CreatedAtUtc,
            };
        }

        private static ConversationThreadDto MapThread(Guid userId, Conversation conversation)
        {
            var otherUser = conversation.ClientId == userId ? conversation.Freelancer : conversation.Client;

            return new ConversationThreadDto
            {
                Id = conversation.Id,
                OtherUserId = otherUser.Id,
                OtherUserName = otherUser.FullName,
                OtherUserRole = otherUser.Role,
                Subject = conversation.Subject,
                Messages = conversation.Messages
                    .OrderBy(message => message.CreatedAtUtc)
                    .Select(message => new MessageDto
                    {
                        Id = message.Id,
                        SenderId = message.SenderId,
                        SenderName = message.Sender.FullName,
                        Content = message.Content,
                        CreatedAtUtc = message.CreatedAtUtc,
                    })
                    .ToList(),
            };
        }
    }
}
