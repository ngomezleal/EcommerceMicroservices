using OrderService.Application.Dtos;

namespace OrderService.Application.Clients;

public interface IProductApiClient
{
    Task<ProductIntegrationDto?> GetProductByIdAsync(int productId);
}
