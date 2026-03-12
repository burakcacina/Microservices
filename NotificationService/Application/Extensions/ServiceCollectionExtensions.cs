using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<NotificationSenderStrategy>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        return services;
    }
}
