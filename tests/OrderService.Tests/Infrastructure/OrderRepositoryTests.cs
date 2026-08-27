using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Tests.Infrastructure;

public sealed class OrderRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddOrderToDatabase()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new OrderRepository(dbContext);
        var order = new Order("customer-1", [new OrderItem(1, 2, 10m), new OrderItem(2, 1, 5m)]);

        // Act
        var createdOrder = await repository.AddAsync(order);

        // Assert
        createdOrder.Id.Should().BeGreaterThan(0);
        var persistedOrder = await dbContext.Orders.Include(item => item.Items).SingleAsync(item => item.Id == createdOrder.Id);
        persistedOrder.Items.Should().HaveCount(2);
        persistedOrder.TotalAmount.Should().Be(25m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnOrderWithItems()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var order = new Order("customer-1", [new OrderItem(1, 2, 10m)]);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
        var repository = new OrderRepository(dbContext);

        // Act
        var result = await repository.GetByIdAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle();
        result.Items[0].ProductId.Should().Be(1);
        result.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenOrdersExist_ReturnsOrdersOrderedById()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var firstOrder = new Order("customer-1", [new OrderItem(1, 1, 10m)]);
        var secondOrder = new Order("customer-2", [new OrderItem(2, 1, 20m)]);
        await dbContext.Orders.AddRangeAsync(firstOrder, secondOrder);
        await dbContext.SaveChangesAsync();
        var repository = new OrderRepository(dbContext);

        // Act
        var result = (await repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(order => order.Id).Should().BeInAscendingOrder();
        result.Should().OnlyContain(order => order.Items.Count == 1);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenOrderExists_UpdatesPersistedStatus()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var order = new Order("customer-1", [new OrderItem(1, 1, 10m)]);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
        var repository = new OrderRepository(dbContext);

        // Act
        await repository.UpdateStatusAsync(order.Id, OrderStatus.Shipped);

        // Assert
        var updatedOrder = await dbContext.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Shipped);
    }

    private static OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OrderDbContext(options);
    }
}
