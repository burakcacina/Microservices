using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.HostedService;
using Infrastructure.Implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructureDependecy(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("OrderDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:OrderDb");

        builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOutbox, Outbox>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddHostedService<OutboxPublisherHostedService>();

        return builder;
    }
}
