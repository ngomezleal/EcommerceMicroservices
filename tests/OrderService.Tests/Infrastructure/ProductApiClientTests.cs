using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using OrderService.Application.Dtos;
using OrderService.Infrastructure.Clients;

namespace OrderService.Tests.Infrastructure;

public sealed class ProductApiClientTests
{
    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, JsonContent.Create(new ProductIntegrationDto(1, "Laptop", 20m, 3)));
        using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://product-service/") };
        var client = new ProductApiClient(httpClient);

        // Act
        var result = await client.GetProductByIdAsync(1);

        // Assert
        result.Should().Be(new ProductIntegrationDto(1, "Laptop", 20m, 3));
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.NotFound, new StringContent(string.Empty));
        using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://product-service/") };
        var client = new ProductApiClient(httpClient);

        // Act
        var result = await client.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenResponseIsUnsuccessful_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.InternalServerError, new StringContent(string.Empty));
        using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://product-service/") };
        var client = new ProductApiClient(httpClient);

        // Act
        Func<Task> action = async () => await client.GetProductByIdAsync(1);

        // Assert
        await action.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenResponseContentIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = CreateHandler(HttpStatusCode.OK, JsonContent.Create<ProductIntegrationDto?>(null));
        using var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("http://product-service/") };
        var client = new ProductApiClient(httpClient);

        // Act
        Func<Task> action = async () => await client.GetProductByIdAsync(1);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    private static Mock<HttpMessageHandler> CreateHandler(HttpStatusCode statusCode, HttpContent content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(statusCode) { Content = content });

        return handler;
    }
}
