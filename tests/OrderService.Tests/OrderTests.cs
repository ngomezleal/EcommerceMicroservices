using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Tests;

public class OrderTests
{
    [Fact]
    public void CalculateTotal_WithMultipleItems_SetsExpectedAmount()
    {
        // Arrange
        var order = new Order("customer-1", [new OrderItem(1, 2, 10m), new OrderItem(2, 3, 5m)]);

        // Act
        order.CalculateTotal();

        // Assert
        order.TotalAmount.Should().Be(35m);
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_UpdatesStatus()
    {
        // Arrange
        var order = new Order("customer-1", [new OrderItem(1, 1, 10m)]);

        // Act
        order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        order.Status.Should().Be(OrderStatus.Shipped);
    }
}
