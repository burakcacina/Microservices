using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.HostedService;
using Infrastructure.Implementation;

namespace Infrastructure;

public static class DependencyInjection
{
      public static IHostApplicationBuilder AddInfrastructureDependecy(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("NotificationDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:NotificationDb");

        builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddScoped<IEmailNotificationSender, EmailSender>();
        builder.Services.AddScoped<ISmsNotificationSender, SmsSender>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddHostedService<RabbitMqConsumerHostedService>();

        return builder;
    }
}
