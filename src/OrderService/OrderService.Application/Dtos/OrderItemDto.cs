namespace OrderService.Application.Dtos;

public record OrderItemDto(int Id, int ProductId, int Quantity, decimal UnitPrice);
