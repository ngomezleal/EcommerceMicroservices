using OrderService.Application.Dtos;
using OrderService.Domain.Entities;

namespace OrderService.Application.Mappings;

public static class OrderMappings
{
    public static OrderDto ToDto(this Order order) => new(
        order.Id, order.CustomerId, order.Status, order.TotalAmount, order.OrderDate,
        order.Items.Select(item => new OrderItemDto(item.Id, item.ProductId, item.Quantity, item.UnitPrice)).ToList());
}
