using Freelance.Application.DTOs.Notifications;
using Freelance.Application.Interfaces;
using Freelance.Domain.Entities;
using Freelance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Freelance.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId)
    {
        var notifications = await _db.Notifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .ToListAsync();

        return notifications.Select(Map);
    }

    public async Task<NotificationSummaryDto> GetSummaryAsync(Guid userId)
    {
        return new NotificationSummaryDto
        {
            UnreadCount = await _db.Notifications.CountAsync(notification => notification.UserId == userId && !notification.IsRead),
        };
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId);
        if (notification == null)
        {
            throw new InvalidOperationException("Notification not found");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            notification.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _db.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
            notification.UpdatedAtUtc = now;
        }

        await _db.SaveChangesAsync();
    }

    public async Task CreateAsync(Guid userId, string type, string title, string message, string link = "")
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type.Trim(),
            Title = title.Trim(),
            Message = message.Trim(),
            Link = link.Trim(),
        });

        await _db.SaveChangesAsync();
    }

    private static NotificationDto Map(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Link = notification.Link,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAtUtc,
        };
    }
}
