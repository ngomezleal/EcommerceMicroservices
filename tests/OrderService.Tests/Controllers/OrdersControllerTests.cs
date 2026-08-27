using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Api.Controllers;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Queries;
using OrderService.Domain.Enums;

namespace OrderService.Tests.Controllers;

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task GetOrders_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        IEnumerable<OrderDto> orders = [CreateOrderDto(1), CreateOrderDto(2)];
        sender
            .Setup(service => service.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        var controller = new OrdersController(sender.Object);

        // Act
        var result = await controller.GetOrdersAsync();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeSameAs(orders);
        sender.Verify(
            service => service.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderExists_ReturnsOkResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var order = CreateOrderDto(1);
        sender
            .Setup(service => service.Send(new GetOrderByIdQuery(order.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        var controller = new OrdersController(sender.Object);

        // Act
        var result = await controller.GetOrderByIdAsync(order.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(order);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedAtRouteResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 2)]);
        var order = CreateOrderDto(1);
        sender
            .Setup(service => service.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        var controller = new OrdersController(sender.Object);

        // Act
        var result = await controller.CreateOrderAsync(command);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.RouteName.Should().Be(nameof(OrdersController.GetOrderByIdAsync));
        createdResult.RouteValues!["id"].Should().Be(order.Id);
        createdResult.Value.Should().NotBeNull();
        createdResult.Value.Should().Be(order);
        sender.Verify(
            service => service.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenOrderExists_ReturnsNoContentResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var command = new UpdateOrderStatusCommand(99, OrderStatus.Confirmed);
        sender
            .Setup(service => service.Send(It.Is<UpdateOrderStatusCommand>(request => request.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new OrdersController(sender.Object);

        // Act
        var result = await controller.UpdateOrderStatusAsync(1, command);

        // Assert
        var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
        noContentResult.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        sender.Verify(
            service => service.Send(It.Is<UpdateOrderStatusCommand>(request => request.Id == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sender = new Mock<ISender>();
        sender
            .Setup(service => service.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);
        var controller = new OrdersController(sender.Object);

        // Act
        Func<Task> action = async () => await controller.GetOrderByIdAsync(999);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    private static OrderDto CreateOrderDto(int id)
    {
        return new OrderDto(id, "customer-1", OrderStatus.Pending, 20m, DateTime.UtcNow, []);
    }
}
