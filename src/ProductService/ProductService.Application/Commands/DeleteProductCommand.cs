using MediatR;

namespace ProductService.Application.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;
