using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetAllAsync() => await dbContext.Orders.AsNoTracking().Include(order => order.Items).OrderBy(order => order.Id).ToListAsync();
    public Task<Order?> GetByIdAsync(int id) => dbContext.Orders.Include(order => order.Items).FirstOrDefaultAsync(order => order.Id == id);
    public async Task<Order> AddAsync(Order order)
    {
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
        return order;
    }
    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await dbContext.Orders.FindAsync(id);
        if (order is null) return;
        order.UpdateStatus(status);
        await dbContext.SaveChangesAsync();
    }
}
