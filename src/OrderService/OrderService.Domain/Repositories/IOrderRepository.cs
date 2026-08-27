using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> AddAsync(Order order);
    Task UpdateStatusAsync(int id, OrderStatus status);
}
