using MediatR;

namespace ProductService.Application.Commands;

public record UpdateProductCommand(int Id, string Name, string Description, decimal Price, int Stock) : IRequest<bool>;
