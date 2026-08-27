using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.Dtos;
using OrderService.Application.Queries;
using OrderService.Domain.Enums;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersAsync() => Ok(await sender.Send(new GetOrdersQuery()));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync(int id)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id));
        if (order is null) throw new KeyNotFoundException($"Order with id {id} was not found.");
        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrderAsync([FromBody] CreateOrderCommand command)
    {
        var order = await sender.Send(command);
        return CreatedAtAction(nameof(GetOrderByIdAsync), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatusAsync(int id, [FromBody] UpdateOrderStatusCommand command)
    {
        if (!await sender.Send(command with { Id = id })) throw new KeyNotFoundException($"Order with id {id} was not found.");
        return NoContent();
    }
}
