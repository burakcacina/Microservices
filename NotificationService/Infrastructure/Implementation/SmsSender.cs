using Microsoft.Extensions.Logging;
using Application.Interfaces;

namespace Infrastructure.Implementation;

public sealed class SmsSender(ILogger<SmsSender> logger) : ISmsNotificationSender
{
    public Task SendAsync(string to, string message, string? IdempotencyKey, CancellationToken ct)
    {
        logger.LogInformation("[SMS] message sent to='{To}', idempotencyKey='{IdempotencyKey}'.", to, IdempotencyKey);
        Console.WriteLine($"[SMS] message sent to='{to}', idempotencyKey='{IdempotencyKey}'");
        return Task.CompletedTask;
    }
}
