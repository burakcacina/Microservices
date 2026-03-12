using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;

namespace Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static async Task EnsureNotificationDatabaseCreatedAsync(this WebApplication app, CancellationToken ct = default)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            await db.Database.EnsureCreatedAsync(ct);

            app.Logger.LogInformation("Database initialization completed.");
        }
        catch (OperationCanceledException)
        {
            app.Logger.LogError("Database initialization failed. Check Postgres connectivity/credentials");
            throw;
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
