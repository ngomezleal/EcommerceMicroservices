using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using OrderService.Application.Clients;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Clients;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using Polly;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        var productServiceUrl = configuration["ProductServiceUrl"]
            ?? throw new InvalidOperationException("ProductServiceUrl is not configured.");

        services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddHttpClient<IProductApiClient, ProductApiClient>(client => client.BaseAddress = new Uri(productServiceUrl))
            .AddResilienceHandler("product-api", static builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = false
                });
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 1,
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
            });

        return services;
    }
}
