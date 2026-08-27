using ProductService.Application.Dtos;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappings;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.CreatedAt,
        product.UpdatedAt);
}
