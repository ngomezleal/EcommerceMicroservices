using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Dtos;
using ProductService.Application.Mappings;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class CreateProductCommandHandler(IProductRepository productRepository)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Description, request.Price, request.Stock);
        var createdProduct = await productRepository.AddAsync(product);

        return createdProduct.ToDto();
    }
}
