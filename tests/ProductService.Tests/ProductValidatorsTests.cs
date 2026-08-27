using FluentAssertions;
using ProductService.Application.Commands;
using ProductService.Application.Validators;

namespace ProductService.Tests;

public class ProductValidatorsTests
{
    [Fact]
    public void CreateProduct_InvalidNamePriceAndStock_ReturnsValidationErrors()
    {
        // Arrange
        var validator = new CreateProductCommandValidator();

        // Act
        var result = validator.Validate(new CreateProductCommand(string.Empty, "Description", 0m, -1));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void UpdateProduct_InvalidIdAndLongName_ReturnsValidationErrors()
    {
        // Arrange
        var validator = new UpdateProductCommandValidator();

        // Act
        var result = validator.Validate(new UpdateProductCommand(0, new string('a', 101), "Description", 1m, 0));

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
