using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
