using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApplicationDependecy(this IHostApplicationBuilder builder)
    {
        builder.Services.AddNotificationApplicationServices();
        return builder;
    }
}
