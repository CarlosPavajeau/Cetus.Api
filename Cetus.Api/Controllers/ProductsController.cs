using Cetus.Application.CreateProduct;
using Cetus.Application.DeleteProduct;
using Cetus.Application.FindProduct;
using Cetus.Application.SearchAllProducts;
using Cetus.Application.SearchAllProductsForSale;
using Cetus.Application.UpdateProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var created = await _mediator.Send(command);

        return Ok(created);
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _mediator.Send(new SearchAllProductsQuery());

        return Ok(products);
    }
    
    [HttpGet("for-sale")]
    public async Task<IActionResult> GetProductsForSale()
    {
        var products = await _mediator.Send(new SearchAllProductsForSaleQuery());

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _mediator.Send(new FindProductQuery(id));

        return product is null
            ? NotFound()
            : Ok(product);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var updated = await _mediator.Send(command);

        return updated is null
            ? NotFound()
            : Ok(updated);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteProductCommand(id));

        return deleted
            ? Ok()
            : NotFound();
    }
}
