using MediatR;
using OrderService.Domain.Enums;

namespace OrderService.Application.Commands;

public record UpdateOrderStatusCommand(int Id, OrderStatus Status) : IRequest<bool>;
