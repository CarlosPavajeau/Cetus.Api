using Cetus.Application.CreateProduct;
using Cetus.Application.SearchAllProducts;
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
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command)
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
}
