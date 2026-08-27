namespace OrderService.Application.Commands;

public record CreateOrderItemCommand(int ProductId, int Quantity);
