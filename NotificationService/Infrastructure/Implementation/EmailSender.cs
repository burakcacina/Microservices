using Microsoft.Extensions.Logging;
using Application.Interfaces;

namespace Infrastructure.Implementation;

public sealed class EmailSender(ILogger<EmailSender> logger) : IEmailNotificationSender
{
    public Task SendAsync(string to, string message, string? IdempotencyKey, CancellationToken ct)
    {
        logger.LogInformation("[EMAIL] message sent to='{To}', idempotencyKey='{IdempotencyKey}'.", to, IdempotencyKey);
        Console.WriteLine($"[EMAIL] message sent to='{to}', idempotencyKey='{IdempotencyKey}'");
        return Task.CompletedTask;
    }
}
