using MediatR;
using OrderService.Application.Dtos;
using OrderService.Application.Mappings;
using OrderService.Application.Queries;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class GetOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync();
        return orders.Select(order => order.ToDto()).ToList();
    }
}
