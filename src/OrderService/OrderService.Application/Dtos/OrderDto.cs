using OrderService.Domain.Enums;

namespace OrderService.Application.Dtos;

public record OrderDto(
    int Id,
    string CustomerId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderDate,
    List<OrderItemDto> Items);
