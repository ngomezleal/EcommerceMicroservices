using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.Queries;

public record GetProductsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResultDto<ProductDto>>;
