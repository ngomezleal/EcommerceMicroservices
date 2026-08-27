using FluentAssertions;
using Moq;
using OrderService.Application.Commands;
using OrderService.Application.Handlers;
using OrderService.Application.Queries;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;

namespace OrderService.Tests.Unit;

public sealed class OrderHandlersTests
{
    [Fact]
    public async Task Handle_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        var order = new Order("customer-1", [new OrderItem(1, 2, 10m)]);
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync(order);
        var handler = new GetOrderByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be("customer-1");
        result.TotalAmount.Should().Be(20m);
        result.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync((Order?)null);
        var handler = new GetOrderByIdQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOrdersExist_ReturnsMappedOrderDtos()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        repository.Setup(item => item.GetAllAsync()).ReturnsAsync([
            new Order("customer-1", [new OrderItem(1, 1, 10m)]),
            new Order("customer-2", [new OrderItem(2, 2, 15m)])]);
        var handler = new GetOrdersQueryHandler(repository.Object);

        // Act
        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(order => order.TotalAmount).Should().Equal(10m, 30m);
    }

    [Fact]
    public async Task Handle_WhenOrderExists_UpdatesStatusAndReturnsTrue()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        repository.Setup(item => item.GetByIdAsync(1))
            .ReturnsAsync(new Order("customer-1", [new OrderItem(1, 1, 10m)]));
        var handler = new UpdateOrderStatusCommandHandler(repository.Object);
        var command = new UpdateOrderStatusCommand(1, OrderStatus.Shipped);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repository.Verify(item => item.UpdateStatusAsync(command.Id, command.Status), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repository = new Mock<IOrderRepository>();
        repository.Setup(item => item.GetByIdAsync(1)).ReturnsAsync((Order?)null);
        var handler = new UpdateOrderStatusCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(new UpdateOrderStatusCommand(1, OrderStatus.Shipped), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        repository.Verify(item => item.UpdateStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>()), Times.Never);
    }
}
