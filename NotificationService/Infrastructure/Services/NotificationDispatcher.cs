using Application.Interfaces;
using Microsoft.Extensions.Logging;
using NotificationService.Domain;

namespace Infrastructure.Services;

public sealed class NotificationDispatcher(
    INotificationRepository notificationRepository,
    NotificationSenderStrategy notificationSenderStrategy,
    ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    public async Task DispatchAsync(Notification notification, CancellationToken ct)
    {
        try
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsSent = false;
            notification.SentAt = null;
            notification.Error = null;

            if (notificationRepository.GetAll().Any(x => notification.IdempotencyKey == x.IdempotencyKey && x.Channel == notification.Channel))
            {
                logger.LogInformation(
                    "Notification with IdempotencyKey='{IdempotencyKey}' already exists for channel '{Channel}'. Skipping dispatch.",
                    notification.IdempotencyKey,
                    notification.Channel);
                return;
            }

            await notificationRepository.AddAsync(notification, ct);
            await notificationRepository.SaveChangesAsync(ct);

            await notificationSenderStrategy.SendAsync(notification, ct);

            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
            try { await notificationRepository.SaveChangesAsync(ct); }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to update notification status."); }
        }
        catch (Exception ex)
        {
            notification.IsSent = false;
            notification.Error = ex.Message;
            try { await notificationRepository.SaveChangesAsync(ct); }
            catch (Exception dbEx) { logger.LogWarning(dbEx, "Failed to save notification error."); }
            throw;
        }
    }
}