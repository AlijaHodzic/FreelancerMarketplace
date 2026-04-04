using Freelance.Application.DTOs.Notifications;

namespace Freelance.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId);
    Task<NotificationSummaryDto> GetSummaryAsync(Guid userId);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task CreateAsync(Guid userId, string type, string title, string message, string link = "");
}
