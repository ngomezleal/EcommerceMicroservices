using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.CustomerId).IsRequired().HasMaxLength(100);
            entity.Property(order => order.Status).IsRequired();
            entity.Property(order => order.TotalAmount).HasPrecision(18, 2);
            entity.Property(order => order.OrderDate).IsRequired();
            entity.HasMany(order => order.Items).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ProductId).IsRequired();
            entity.Property(item => item.Quantity).IsRequired();
            entity.Property(item => item.UnitPrice).HasPrecision(18, 2);
        });
    }
}
