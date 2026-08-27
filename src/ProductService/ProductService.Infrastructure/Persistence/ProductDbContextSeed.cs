using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Infrastructure.Persistence;

public static class ProductDbContextSeed
{
    public static async Task SeedAsync(ProductDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
        {
            return;
        }

        var products = new[]
        {
            new Product("Laptop Pro", "Laptop profesional de alto rendimiento", 1299.99m, 15),
            new Product("Mouse inalámbrico", "Mouse ergonómico con conexión Bluetooth", 29.99m, 50),
            new Product("Teclado mecánico", "Teclado mecánico retroiluminado", 89.99m, 30)
        };

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();
    }
}
