using FluentAssertions;
using OrderService.Application.Commands;
using OrderService.Application.Validators;

namespace OrderService.Tests;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public void Validate_EmptyCustomerId_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(string.Empty, [new CreateOrderItemCommand(1, 1)]);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ZeroQuantity_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand("customer-1", [new CreateOrderItemCommand(1, 0)]);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
