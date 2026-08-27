using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Api.Controllers;
using ProductService.Application.Commands;
using ProductService.Application.Dtos;
using ProductService.Application.Queries;

namespace ProductService.Tests;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task CreateProductAsync_WithValidCommand_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var product = CreateProductDto();
        sender
            .Setup(service => service.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.CreateProductAsync(new CreateProductCommand("Product", "Description", 10m, 5));

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetProductByIdAsync));
        createdResult.RouteValues!["id"].Should().Be(product.Id);
        createdResult.Value.Should().Be(product);
    }

    [Fact]
    public async Task UpdateProductAsync_WithDifferentBodyId_UsesRouteId()
    {
        // Arrange
        var sender = new Mock<ISender>();
        sender
            .Setup(service => service.Send(It.Is<UpdateProductCommand>(command => command.Id == 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.UpdateProductAsync(5, new UpdateProductCommand(99, "Product", "Description", 10m, 5));

        // Assert
        result.Should().BeOfType<NoContentResult>();
        sender.Verify(
            service => service.Send(It.Is<UpdateProductCommand>(command => command.Id == 5), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sender = new Mock<ISender>();
        sender
            .Setup(service => service.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductDto?)null);
        var controller = new ProductsController(sender.Object);

        // Act
        Func<Task> action = async () => await controller.GetProductByIdAsync(999);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static ProductDto CreateProductDto()
    {
        return new ProductDto(1, "Product", "Description", 10m, 5, DateTime.UtcNow, DateTime.UtcNow);
    }
}
