using Infrastructure.Data;
using Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static async Task EnsureStockDatabaseCreatedAsync(this WebApplication app)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var seederLogger = loggerFactory.CreateLogger("StockDbSeeder");
            await db.Database.EnsureCreatedAsync();
            await StockDbSeeder.SeedAsync(db, seederLogger);

            app.Logger.LogInformation("Database initialization and seeding completed.");
        }
        catch (OperationCanceledException)
        {
            app.Logger.LogError(
                "Database initialization failed. Check Postgres connectivity/credentials");
            throw;
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
