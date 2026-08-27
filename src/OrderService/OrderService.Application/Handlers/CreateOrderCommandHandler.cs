using FluentValidation;
using MediatR;
using OrderService.Application.Clients;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Mappings;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IProductApiClient productApiClient) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var products = new Dictionary<int, ProductIntegrationDto>();
        foreach (var group in request.Items.GroupBy(item => item.ProductId))
        {
            var product = await productApiClient.GetProductByIdAsync(group.Key)
                ?? throw new ValidationException($"Product with id {group.Key} was not found.");
            if (product.Stock < group.Sum(item => item.Quantity))
            {
                throw new ValidationException($"Product with id {group.Key} does not have enough stock.");
            }

            products.Add(product.Id, product);
        }

        var orderItems = request.Items.Select(item => new OrderItem(item.ProductId, item.Quantity, products[item.ProductId].Price)).ToList();
        var createdOrder = await orderRepository.AddAsync(new Order(request.CustomerId, orderItems));
        return createdOrder.ToDto();
    }
}
