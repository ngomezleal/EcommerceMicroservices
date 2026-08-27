using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrderService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(OrderDbContext))]
partial class OrderDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11").HasAnnotation("Relational:MaxIdentifierLength", 63);
        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("OrderService.Domain.Entities.Order", order =>
        {
            order.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            order.Property<string>("CustomerId").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            order.Property<DateTime>("OrderDate").HasColumnType("timestamp with time zone");
            order.Property<int>("Status").HasColumnType("integer");
            order.Property<decimal>("TotalAmount").HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            order.HasKey("Id");
            order.ToTable("Orders");
        });

        modelBuilder.Entity("OrderService.Domain.Entities.OrderItem", item =>
        {
            item.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer").HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            item.Property<int>("OrderId").HasColumnType("integer");
            item.Property<int>("ProductId").HasColumnType("integer");
            item.Property<int>("Quantity").HasColumnType("integer");
            item.Property<decimal>("UnitPrice").HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            item.HasKey("Id");
            item.HasIndex("OrderId");
            item.ToTable("OrderItems");
        });

        modelBuilder.Entity("OrderService.Domain.Entities.OrderItem", item =>
        {
            item.HasOne("OrderService.Domain.Entities.Order", null).WithMany("Items").HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        });
    }
}
