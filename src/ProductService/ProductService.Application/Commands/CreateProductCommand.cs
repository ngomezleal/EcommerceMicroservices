using MediatR;
using ProductService.Application.Dtos;

namespace ProductService.Application.Commands;

public record CreateProductCommand(string Name, string Description, decimal Price, int Stock) : IRequest<ProductDto>;
