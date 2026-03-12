using FluentValidation;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Models.Request;
using Application.Validation;
using Application.Services;

namespace Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IOrderService, Services.OrderService>();
        return services;
    }

    public static IServiceCollection AddStockServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration.GetValue<string>("StockService:BaseUrl") ?? "http://localhost:5052/";

        services.AddHttpClient("StockService", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
