using Microsoft.AspNetCore.SignalR;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;

namespace TLearn.Infrastructure.Services;

public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        string? metadata = null);

    Task CreateManyAsync(
        List<Guid> userIds,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        string? metadata = null);
}

public class NotificationService : INotificationService

{
    private readonly TLearnDbContext _context;

    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        TLearnDbContext context,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        string? metadata = null)

    {
        var notification = new Notification

        {
            UserId = userId,

            Title = title,

            Message = message,

            Type = type,

            ActionUrl = actionUrl,

            Metadata = metadata
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        // Realtime

        await _hubContext.Clients
            .User(userId.ToString())
            .SendAsync(
                "NotificationReceived",
                new

                {
                    notification.Id,

                    notification.Title,

                    notification.Message,

                    Type = notification.Type.ToString(),

                    notification.ActionUrl,

                    notification.CreatedAt
                });
    }

    public async Task CreateManyAsync(
        List<Guid> userIds,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null,
        string? metadata = null)

    {
        if (userIds.Count == 0)

        {
            return;
        }

        var distinctUserIds = userIds
            .Distinct()
            .ToList();

        var now = DateTime.UtcNow;

        var notifications = distinctUserIds
            .Select(userId => new Notification

            {
                UserId = userId,

                Title = title,

                Message = message,

                Type = type,

                ActionUrl = actionUrl,

                Metadata = metadata,

                CreatedAt = now
            })
            .ToList();

        await _context.Notifications
            .AddRangeAsync(notifications);

        await _context.SaveChangesAsync();

        // Realtime tất cả user

        var realtimeTasks = notifications
            .Select(notification =>
                _hubContext.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync(
                        "NotificationReceived",
                        new

                        {
                            notification.Id,

                            notification.Title,

                            notification.Message,

                            Type = notification.Type.ToString(),

                            notification.ActionUrl,

                            notification.CreatedAt
                        }))
            .ToList();

        await Task.WhenAll(realtimeTasks);
    }
}