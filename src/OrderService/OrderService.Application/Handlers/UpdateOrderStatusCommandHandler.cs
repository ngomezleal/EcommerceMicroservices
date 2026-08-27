using MediatR;
using OrderService.Application.Commands;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Handlers;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository) : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (await orderRepository.GetByIdAsync(request.Id) is null)
        {
            return false;
        }

        await orderRepository.UpdateStatusAsync(request.Id, request.Status);
        return true;
    }
}
