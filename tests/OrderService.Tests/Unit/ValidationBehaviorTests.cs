using FluentAssertions;
using FluentValidation;
using OrderService.Application.Behaviors;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Validators;

namespace OrderService.Tests.Unit;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidCreateOrderCommand_ThrowsValidationException()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateOrderCommand, OrderDto>([new CreateOrderCommandValidator()]);
        var command = new CreateOrderCommand(string.Empty, []);

        // Act
        Func<Task> action = async () => await behavior.Handle(
            command,
            () => Task.FromResult(new OrderDto(1, "customer-1", 0, 0m, DateTime.UtcNow, [])),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithoutValidators_ExecutesNextHandler()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateOrderCommand, OrderDto>([]);
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 1)]);
        var expectedOrder = new OrderDto(1, "customer-1", 0, 10m, DateTime.UtcNow, []);

        // Act
        var result = await behavior.Handle(command, () => Task.FromResult(expectedOrder), CancellationToken.None);

        // Assert
        result.Should().Be(expectedOrder);
    }
}
