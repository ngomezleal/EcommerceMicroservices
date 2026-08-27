using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Mappings;
using ProductService.Application.Queries;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);

        return product?.ToDto();
    }
}
