namespace Application.Interfaces;
public interface INotificationSender
{
    Task SendAsync(string to, string message, string? IdempotencyKey, CancellationToken ct);
}
