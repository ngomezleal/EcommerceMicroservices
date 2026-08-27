using FluentAssertions;
using FluentValidation;
using Moq;
using OrderService.Application.Clients;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Tests.Unit;

public sealed class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidProductAndStock_ShouldCreateOrderWithPendingStatus()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        var productApiClient = new Mock<IProductApiClient>();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 2)]);
        productApiClient.Setup(client => client.GetProductByIdAsync(1))
            .ReturnsAsync(new ProductIntegrationDto(1, "Laptop", 15m, 5));
        repository.Setup(item => item.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order order) => order);
        var handler = new CreateOrderCommandHandler(repository.Object, productApiClient.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(OrderStatus.Pending);
        result.TotalAmount.Should().Be(30m);
        result.Items.Should().ContainSingle();
        result.Items[0].ProductId.Should().Be(1);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(15m);
        repository.Verify(
            item => item.AddAsync(It.Is<Order>(order =>
                order.CustomerId == command.CustomerId &&
                order.Status == OrderStatus.Pending &&
                order.TotalAmount == 30m)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowValidationException()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        var productApiClient = new Mock<IProductApiClient>();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 1)]);
        productApiClient.Setup(client => client.GetProductByIdAsync(1))
            .ReturnsAsync((ProductIntegrationDto?)null);
        var handler = new CreateOrderCommandHandler(repository.Object, productApiClient.Object);

        // Act
        Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
        repository.Verify(item => item.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenStockIsInsufficient_ShouldThrowValidationException()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        var productApiClient = new Mock<IProductApiClient>();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 10)]);
        productApiClient.Setup(client => client.GetProductByIdAsync(1))
            .ReturnsAsync(new ProductIntegrationDto(1, "Laptop", 15m, 2));
        var handler = new CreateOrderCommandHandler(repository.Object, productApiClient.Object);

        // Act
        Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
        repository.Verify(item => item.AddAsync(It.IsAny<Order>()), Times.Never);
    }
}
