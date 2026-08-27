using FluentValidation;
using ProductService.Application.Commands;

namespace ProductService.Application.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.Stock).GreaterThanOrEqualTo(0);
    }
}
