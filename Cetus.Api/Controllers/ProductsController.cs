using Cetus.Products.Application.Create;
using Cetus.Products.Application.Delete;
using Cetus.Products.Application.Find;
using Cetus.Products.Application.SearchAll;
using Cetus.Products.Application.SearchForSale;
using Cetus.Products.Application.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly HybridCache _cache;

    public ProductsController(IMediator mediator, HybridCache cache)
    {
        _mediator = mediator;
        _cache = cache;
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
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsForSale()
    {
        var products = await _cache.GetOrCreateAsync(
            "products-for-sale",
            async cancellationToken => await _mediator.Send(new SearchAllProductsForSaleQuery(), cancellationToken)
        );

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _cache.GetOrCreateAsync(
            $"product-{id}",
            async cancellationToken => await _mediator.Send(new FindProductQuery(id), cancellationToken)
        );

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

        if (updated is null)
        {
            return NotFound();
        }
        
        await _cache.RemoveAsync($"product-{id}");
        
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteProductCommand(id));

        if (!deleted)
        {
            return NotFound();
        }

        await _cache.RemoveAsync($"product-{id}");
        await _cache.RemoveAsync("products-for-sale");

        return Ok(new {Id = id});
    }
}
