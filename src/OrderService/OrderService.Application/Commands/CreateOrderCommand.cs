using MediatR;
using OrderService.Application.Dtos;

namespace OrderService.Application.Commands;

public record CreateOrderCommand(string CustomerId, List<CreateOrderItemCommand> Items) : IRequest<OrderDto>;
