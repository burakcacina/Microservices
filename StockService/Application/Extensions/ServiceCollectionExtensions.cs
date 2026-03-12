using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStockApplicationServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddStockServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration.GetValue<string>("StockService:BaseUrl") ?? "http://localhost:5159/";
        services.AddHttpClient("StockService", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
