using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];

    public Order()
    {
    }

    public Order(string customerId, List<OrderItem> items)
    {
        CustomerId = customerId;
        Items = items;
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        CalculateTotal();
    }

    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.Quantity * item.UnitPrice);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}
