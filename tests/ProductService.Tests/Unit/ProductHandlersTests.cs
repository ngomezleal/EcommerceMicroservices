using FluentAssertions;
using Moq;
using ProductService.Application.Commands;
using ProductService.Application.Handlers;
using ProductService.Application.Queries;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Tests.Unit;

public sealed class ProductHandlersTests
{
    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync(new Product("Laptop", "Portable", 1000m, 3));
        var handler = new GetProductByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync((Product?)null);
        var handler = new GetProductByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsPagedResult()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetAllAsync(2, 1)).ReturnsAsync(new List<Product> { new("Laptop", "Portable", 1000m, 3) });
        repository.Setup(item => item.GetTotalCountAsync()).ReturnsAsync(2);
        var handler = new GetProductsQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetProductsQuery(2, 1), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(2);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateProduct_WhenProductExists_ReturnsTrue()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        var product = new Product("Laptop", "Portable", 1000m, 3);
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync(product);
        var handler = new UpdateProductCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(new UpdateProductCommand(1, "Tablet", "Nueva", 700m, 5), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        product.Name.Should().Be("Tablet");
        repository.Verify(item => item.UpdateAsync(product), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync((Product?)null);
        var handler = new UpdateProductCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(new UpdateProductCommand(1, "Tablet", "Nueva", 700m, 5), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        repository.Verify(item => item.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync((Product?)null);
        var handler = new DeleteProductCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(1), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        repository.Verify(item => item.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_ReturnsTrue()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync(new Product("Laptop", "Portable", 1000m, 3));
        var handler = new DeleteProductCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(1), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repository.Verify(item => item.DeleteAsync(1), Times.Once);
    }
}
