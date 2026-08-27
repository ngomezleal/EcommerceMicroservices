using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Api.Controllers;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Domain.Enums;

namespace OrderService.Tests.Controllers;

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedRouteResult()
    {
        // Arrange
        var sender = new Mock<ISender>();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 2)]);
        var order = new OrderDto(1, "customer-1", OrderStatus.Pending, 20m, DateTime.UtcNow, []);
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
        createdResult.Value.Should().Be(order);
        sender.Verify(
            service => service.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
