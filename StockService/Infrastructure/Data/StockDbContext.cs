using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class StockDbContext(DbContextOptions<StockDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Stock> ProductStocks => Set<Stock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();
        product.ToTable("products");
        product.HasKey(x => x.Id);

        product.Property(x => x.Name).IsRequired();
        product.Property(x => x.Description);
        product.Property(x => x.Category);
        product.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        var stock = modelBuilder.Entity<Stock>();
        stock.ToTable("product_stocks");
        stock.HasKey(x => x.Id);
        stock.Property(x => x.ProductId).IsRequired();
        stock.Property(x => x.Quantity).IsRequired();
        stock.Property(x => x.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        stock.HasIndex(x => x.ProductId).IsUnique();

        product
            .HasOne(p => p.Stock)
            .WithOne()
            .HasForeignKey<Stock>(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        product.Navigation(x => x.Stock).IsRequired();
    }
}
