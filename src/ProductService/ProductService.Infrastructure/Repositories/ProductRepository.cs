using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository(ProductDbContext dbContext) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync(int page, int pageSize)
    {
        return await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<int> GetTotalCountAsync() => dbContext.Products.CountAsync();

    public Task<Product?> GetByIdAsync(int id) => dbContext.Products.FirstOrDefaultAsync(product => product.Id == id);

    public async Task<Product> AddAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await dbContext.Products.FindAsync(id);
        if (product is null)
        {
            return;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync();
    }
}
