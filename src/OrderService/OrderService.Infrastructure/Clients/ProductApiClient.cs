using System.Net;
using System.Net.Http.Json;
using OrderService.Application.Clients;
using OrderService.Application.Dtos;

namespace OrderService.Infrastructure.Clients;

public class ProductApiClient(HttpClient httpClient) : IProductApiClient
{
    public async Task<ProductIntegrationDto?> GetProductByIdAsync(int productId)
    {
        using var response = await httpClient.GetAsync($"api/products/{productId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductIntegrationDto>()
            ?? throw new InvalidOperationException("ProductService returned an empty product response.");
    }
}
