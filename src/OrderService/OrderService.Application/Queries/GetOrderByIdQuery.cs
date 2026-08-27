using MediatR;
using OrderService.Application.Dtos;

namespace OrderService.Application.Queries;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;
