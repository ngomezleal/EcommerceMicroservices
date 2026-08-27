using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ProductDbContext))]
partial class ProductDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("ProductService.Domain.Entities.Product", product =>
        {
            product.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            product.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            product.Property<string>("Description").IsRequired().HasColumnType("text");
            product.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            product.Property<decimal>("Price").HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            product.Property<int>("Stock").HasColumnType("integer");
            product.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone");

            product.HasKey("Id");
            product.ToTable("Products");
        });
    }
}
