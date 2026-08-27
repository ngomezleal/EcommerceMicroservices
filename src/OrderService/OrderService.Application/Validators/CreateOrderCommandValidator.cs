using FluentValidation;
using OrderService.Application.Commands;

namespace OrderService.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.Items).NotNull().NotEmpty();
        RuleForEach(command => command.Items).ChildRules(item => item.RuleFor(orderItem => orderItem.Quantity).GreaterThan(0));
    }
}
