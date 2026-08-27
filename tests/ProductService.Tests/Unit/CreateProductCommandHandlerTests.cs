using FluentAssertions;
using Moq;
using ProductService.Application.Commands;
using ProductService.Application.Handlers;
using ProductService.Application.Validators;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Tests.Unit;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddProductToRepositoryAndReturnDto()
    {
        // Arrange
        var repository = new Mock<IProductRepository>();
        var command = new CreateProductCommand("Laptop", "Portable", 1000m, 3);
        repository
            .Setup(item => item.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product product) => product);
        var handler = new CreateProductCommandHandler(repository.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        repository.Verify(
            item => item.AddAsync(It.Is<Product>(product =>
                product.Name == command.Name &&
                product.Description == command.Description &&
                product.Price == command.Price &&
                product.Stock == command.Stock)),
            Times.Once);
        result.Name.Should().Be(command.Name);
        result.Description.Should().Be(command.Description);
        result.Price.Should().Be(command.Price);
        result.Stock.Should().Be(command.Stock);
    }

    [Fact]
    public void Handle_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand(string.Empty, "Description", -1m, 1);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Select(error => error.PropertyName).Should().Contain(["Name", "Price"]);
    }
}
