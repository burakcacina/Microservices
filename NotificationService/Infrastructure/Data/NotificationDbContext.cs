using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace Infrastructure.Data;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(b =>
        {
            b.ToTable("notifications");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.Channel).HasConversion<string>().IsRequired().HasMaxLength(16);
            b.Property(x => x.Recipient).IsRequired().HasMaxLength(256);
            b.Property(x => x.Message).IsRequired().HasMaxLength(4000);

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsSent).IsRequired();
            b.Property(x => x.SentAt);
            b.Property(x => x.Error).HasMaxLength(1000);
            b.Property(x => x.IdempotencyKey).HasMaxLength(128);

            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => new { x.Channel, x.IsSent });
            b.HasIndex(x => new { x.IdempotencyKey, x.Channel }).IsUnique();
        });
    }
}
