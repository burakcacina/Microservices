using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Application.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddOrderSerilog(this WebApplicationBuilder builder)
    {
        var seqServerUrl = builder.Configuration["Seq:ServerUrl"];
        var seqApiKey = builder.Configuration["Seq:ApiKey"];
        var orderServiceMinLevel = builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("OrderService", orderServiceMinLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.WithProperty("Application", "OrderService")
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/log.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information);

        if (!string.IsNullOrWhiteSpace(seqServerUrl))
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Seq(
                serverUrl: seqServerUrl,
                apiKey: string.IsNullOrWhiteSpace(seqApiKey) ? null : seqApiKey,
                restrictedToMinimumLevel: LogEventLevel.Information);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}
