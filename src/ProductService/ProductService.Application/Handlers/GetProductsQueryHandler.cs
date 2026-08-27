using MediatR;
using ProductService.Application.Dtos;
using ProductService.Application.Mappings;
using ProductService.Application.Queries;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Handlers;

public class GetProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, PagedResultDto<ProductDto>>
{
    public async Task<PagedResultDto<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(request.Page, request.PageSize);
        var totalCount = await productRepository.GetTotalCountAsync();

        return new PagedResultDto<ProductDto>(
            products.Select(product => product.ToDto()),
            totalCount,
            request.Page,
            request.PageSize);
    }
}
