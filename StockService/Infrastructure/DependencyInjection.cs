using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.HostedService;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructureDependecy(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("StockDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:StockDb");

        builder.Services.AddDbContext<StockDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IStockRepository, StockRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

        return builder;
    }
}
