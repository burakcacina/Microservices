using Microsoft.AspNetCore.Builder;
using Application;
using Application.Extensions;
using Serilog;
using Scalar.AspNetCore;
using Application.Interfaces;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddNotificationSerilog();

Log.Information("Starting web application");

builder.AddApplicationDependecy();
builder.AddInfrastructureDependecy();

builder.Services.AddOpenApi();

var app = builder.Build();

await app.EnsureNotificationDatabaseCreatedAsync();

app.MapScalarApiReference(options =>
{
    options.WithTitle("Notification Service API Documentation")
           .WithTheme(ScalarTheme.BluePlanet);
});

app.MapOpenApi();

var api = app.MapGroup("/api");

api.MapGet("/notifications", async (INotificationRepository repository, CancellationToken ct) =>
{
    var notifications = await repository.GetAll().ToListAsync(ct);
    return Results.Ok(notifications);
});

api.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

await app.RunAsync();
