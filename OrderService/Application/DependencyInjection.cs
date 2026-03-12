using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Extensions;

namespace Application;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApplicationDependecy(this WebApplicationBuilder builder)
    {
        builder.Services.AddOrderApplicationServices();
        builder.Services.AddStockServiceClient(builder.Configuration);

        return builder;
    }
}
