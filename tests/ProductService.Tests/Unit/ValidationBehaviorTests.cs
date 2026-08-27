using FluentAssertions;
using FluentValidation;
using ProductService.Application.Behaviors;
using ProductService.Application.Commands;
using ProductService.Application.Dtos;
using ProductService.Application.Validators;

namespace ProductService.Tests.Unit;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithInvalidCreateProductCommand_ThrowsValidationException()
    {
        // Arrange
        var behavior = new ValidationBehavior<CreateProductCommand, ProductDto>(
            [new CreateProductCommandValidator()]);
        var command = new CreateProductCommand(string.Empty, "Description", 0m, -1);

        // Act
        Func<Task> action = async () => await behavior.Handle(
            command,
            () => Task.FromResult(new ProductDto(1, "Product", "Description", 10m, 1, DateTime.UtcNow, DateTime.UtcNow)),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }
}
