using Application.Interfaces;
using NotificationService.Domain;

namespace Application.Services;

public sealed class NotificationSenderStrategy(
    IEmailNotificationSender emailSender,
    ISmsNotificationSender smsSender)
{
    public Task SendAsync(Notification notification, CancellationToken ct)
    {
        return notification.Channel switch
        {
            NotificationChannel.Email => emailSender.SendAsync(notification.Recipient, notification.Message, notification.IdempotencyKey, ct),
            NotificationChannel.Sms => smsSender.SendAsync(notification.Recipient, notification.Message, notification.IdempotencyKey, ct),
            _ => throw new NotSupportedException($"Notification channel '{notification.Channel}' is not supported.")
        };
    }
}
