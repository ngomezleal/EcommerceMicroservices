using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Api.Controllers;
using ProductService.Application.Commands;
using ProductService.Application.Dtos;
using ProductService.Application.Queries;

namespace ProductService.Tests.Controllers;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task GetProducts_WhenCalled_ReturnsOkWithPagedResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var pagedResult = new PagedResultDto<ProductDto>([CreateProductDto()], 1, 1, 10);
        sender
            .Setup(service => service.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.GetProductsAsync();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(pagedResult);
        sender.Verify(
            service => service.Send(new GetProductsQuery(1, 10), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsOkResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var product = CreateProductDto();
        sender
            .Setup(service => service.Send(new GetProductByIdQuery(product.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.GetProductByIdAsync(product.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(product);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var command = new CreateProductCommand("Product", "Description", 10m, 5);
        var product = CreateProductDto();
        sender
            .Setup(service => service.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.CreateProductAsync(command);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetProductByIdAsync));
        createdResult.RouteValues!["id"].Should().Be(product.Id);
        createdResult.Value.Should().Be(product);
        sender.Verify(
            service => service.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var command = new UpdateProductCommand(1, "Product", "Description", 10m, 5);
        sender
            .Setup(service => service.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.UpdateProductAsync(command.Id, command);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_ReturnsNoContent()
    {
        // Arrange
        var sender = new Mock<ISender>();
        const int productId = 1;
        sender
            .Setup(service => service.Send(new DeleteProductCommand(productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new ProductsController(sender.Object);

        // Act
        var result = await controller.DeleteProductAsync(productId);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task UpdateProduct_WhenBodyIdDiffers_UsesRouteId()
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
    public async Task GetProductById_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
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
