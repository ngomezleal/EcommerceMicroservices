using MediatR;
using OrderService.Application.Dtos;

namespace OrderService.Application.Queries;

public record GetOrdersQuery() : IRequest<IEnumerable<OrderDto>>;
