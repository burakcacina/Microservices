using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Application.Interfaces;
using Serilog;
using Application;
using Application.Extensions;
using Infrastructure;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddStockSerilog();
Log.Information("Starting web application");

builder.Services.AddOpenApi();

builder.AddApplicationDependecy();
builder.AddInfrastructureDependecy();

var app = builder.Build();

await app.EnsureStockDatabaseCreatedAsync();

app.MapScalarApiReference(options =>
{
    options.WithTitle("Stock Service API Documentation")
           .WithTheme(ScalarTheme.BluePlanet);
});

app.MapOpenApi();

var api = app.MapGroup("/api");

api.MapGet("/products", async (IProductRepository repository, CancellationToken ct) =>
{
    var products = await repository.GetAll().ToListAsync(ct);

    var productMapping = products.Select(p => new
    {
        p.Id,
        p.Name,
        Quantity = p.Stock?.Quantity ?? 0
    });

    return Results.Ok(productMapping);
});

api.MapGet("/products/{id}", async (IProductRepository repository, int id, CancellationToken ct) =>
{
    var product = await repository.GetByIdAsync(id, ct);
    if (product is null || product.Stock is null)
    {
        return Results.NotFound();
    }
    var productResult = new
    {
        product.Id,
        product.Name,
        Stock = product.Stock.Quantity
    };
    return Results.Ok(productResult);
});

api.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health");

app.Run();
