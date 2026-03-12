using Scalar.AspNetCore;
using Serilog;
using Application;
using Application.Models.Request;
using Application.Services;
using Application.Extensions;
using Infrastructure;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddOrderSerilog();

Log.Information("Starting web application");

builder.Services.AddOpenApi();

builder.AddApplicationDependecy();
builder.AddInfrastructureDependecy();

var app = builder.Build();

await app.EnsureOrderDatabaseCreatedAsync();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Order Service API Documentation")
           .WithTheme(ScalarTheme.BluePlanet);
});

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

api.MapPost("/orders",
    async (CreateOrderRequest request, IOrderService orderService, CancellationToken ct)
        => await orderService.CreateOrderAsync(request, ct))
    .WithName("CreateOrder");

app.Run();

