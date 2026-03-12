using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static async Task EnsureOrderDatabaseCreatedAsync(this WebApplication app)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            await db.Database.EnsureCreatedAsync();

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
