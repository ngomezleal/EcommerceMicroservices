using FluentValidation;
using ProductService.Application.Commands;

namespace ProductService.Application.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.Stock).GreaterThanOrEqualTo(0);
    }
}
