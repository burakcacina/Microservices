using NotificationService.Domain;

namespace Application.Interfaces;
public interface INotificationDispatcher
{
    Task DispatchAsync(Notification notification, CancellationToken ct);
}
