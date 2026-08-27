using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;
using ProductService.Application.Dtos;
using ProductService.Application.Queries;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProductsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var products = await sender.Send(new GetProductsQuery(page, pageSize));

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await sender.Send(new GetProductByIdQuery(id));

        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> CreateProductAsync([FromBody] CreateProductCommand command)
    {
        var product = await sender.Send(command);

        return CreatedAtAction(nameof(GetProductByIdAsync), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductCommand command)
    {
        var updated = await sender.Send(command with { Id = id });

        if (!updated)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductAsync(int id)
    {
        var deleted = await sender.Send(new DeleteProductCommand(id));

        if (!deleted)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        return NoContent();
    }
}
