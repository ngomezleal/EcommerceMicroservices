using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Tests.Infrastructure;

public sealed class ProductRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new ProductRepository(dbContext);
        var product = new Product("Laptop", "Portable", 1000m, 3);

        // Act
        var createdProduct = await repository.AddAsync(product);

        // Assert
        createdProduct.Id.Should().BeGreaterThan(0);
        (await dbContext.Products.FindAsync(createdProduct.Id)).Should().Be(createdProduct);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var product = new Product("Laptop", "Portable", 1000m, 3);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
        var repository = new ProductRepository(dbContext);

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().Be(product);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPageSize()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await dbContext.Products.AddRangeAsync(
            new Product("A", "A", 1m, 1),
            new Product("B", "B", 2m, 2),
            new Product("C", "C", 3m, 3));
        await dbContext.SaveChangesAsync();
        var repository = new ProductRepository(dbContext);

        // Act
        var products = (await repository.GetAllAsync(2, 1)).ToList();

        // Assert
        products.Should().HaveCount(1);
        products[0].Name.Should().Be("B");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var product = new Product("Laptop", "Portable", 1000m, 3);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
        var repository = new ProductRepository(dbContext);

        // Act
        await repository.DeleteAsync(product.Id);

        // Assert
        (await dbContext.Products.FindAsync(product.Id)).Should().BeNull();
    }

    private static ProductDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProductDbContext(options);
    }
}
