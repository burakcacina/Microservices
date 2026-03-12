namespace NotificationService.Domain;

public enum NotificationChannel
{
    Email = 1,
    Sms = 2
}

public class Notification
{
    public int Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Error { get; set; }
    public string? IdempotencyKey { get; set; }
}
