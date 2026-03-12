using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seed;

public static class StockDbSeeder
{
    public static async Task SeedAsync(StockDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("Seeding database with initial data...");
        var now = DateTime.UtcNow;

        var products = new List<Product>
        {
            new()
            {
                Name = "Classic T-Shirt",
                Description = "Soft cotton, everyday fit",
                Category = "Apparel",
                Price = 15.99m,
                CreatedAt = now,
                Stock = new Stock { Quantity = 100 }
            },
            new()
            {
                Name = "Jacket",
                Description = "Warm and stylish jacket",
                Category = "Apparel",
                Price = 49.99m,
                CreatedAt = now,
                Stock = new Stock { Quantity = 150 }
            },
            new()
            {
                Name = "Sneakers",
                Description = "Comfortable running shoes",
                Category = "Footwear",
                Price = 89.99m,
                CreatedAt = now,
                Stock = new Stock { Quantity = 200 }
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);
    }
}
