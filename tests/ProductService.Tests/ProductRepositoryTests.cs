using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Tests;

public class ProductRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsRequestedPage()
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
        var products = await repository.GetAllAsync(2, 1);

        // Assert
        products.Should().ContainSingle();
        products.Single().Name.Should().Be("B");
    }

    [Fact]
    public async Task AddAsync_AndGetTotalCountAsync_PersistsProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new ProductRepository(dbContext);

        // Act
        var product = await repository.AddAsync(new Product("Laptop", "Portable", 1000m, 3));
        var totalCount = await repository.GetTotalCountAsync();

        // Assert
        product.Id.Should().BeGreaterThan(0);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_AndDeleteAsync_UpdatesThenRemovesProduct()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new ProductRepository(dbContext);
        var product = await repository.AddAsync(new Product("Laptop", "Portable", 1000m, 3));
        product.Update("Tablet", "Nueva", 700m, 5);

        // Act
        await repository.UpdateAsync(product);
        var updatedProduct = await repository.GetByIdAsync(product.Id);
        await repository.DeleteAsync(product.Id);

        // Assert
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Tablet");
        (await repository.GetByIdAsync(product.Id)).Should().BeNull();
    }

    private static ProductDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProductDbContext(options);
    }
}
