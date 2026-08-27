namespace OrderService.Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public OrderItem()
    {
    }

    public OrderItem(int productId, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
