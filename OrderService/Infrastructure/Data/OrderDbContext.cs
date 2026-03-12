using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderUserDetails> OrderUserDetails => Set<OrderUserDetails>();
    public DbSet<Domain.Outbox> OutboxMessages => Set<Domain.Outbox>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<Order>();

        order.ToTable("orders");
        order.HasKey(x => x.Id);

        order.Property(x => x.ProductId).IsRequired();
        order.Property(x => x.Quantity).IsRequired();
        order.Property(x => x.OrderedAt).HasColumnName("ordered_at");

        order.HasOne(x => x.User)
            .WithOne(x => x.Order)
            .HasForeignKey<OrderUserDetails>(x => x.OrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        var user = modelBuilder.Entity<OrderUserDetails>();
        user.ToTable("order_user_details");
        user.HasKey(x => x.Id);
        user.HasIndex(x => x.OrderId).IsUnique();
        user.Property(x => x.PhoneNumber).HasColumnName("phone_number").IsRequired();
        user.Property(x => x.Name).HasColumnName("name").IsRequired();
        user.Property(x => x.Email).HasColumnName("email").IsRequired();
        user.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();

        var outbox = modelBuilder.Entity<Domain.Outbox>();
        outbox.ToTable("outbox");
        outbox.HasKey(x => x.Id);
        outbox.Property(x => x.Type).HasColumnName("type").IsRequired();
        outbox.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        outbox.Property(x => x.OccurredAt).HasColumnName("occurred_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        outbox.Property(x => x.ProcessedAt).HasColumnName("processed_at");
        outbox.Property(x => x.Attempts).HasColumnName("attempts").HasDefaultValue(0);
        outbox.Property(x => x.LastError).HasColumnName("last_error");
        outbox.HasIndex(x => x.ProcessedAt);
        outbox.HasIndex(x => x.OccurredAt);
    }
}
