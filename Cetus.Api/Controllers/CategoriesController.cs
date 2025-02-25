using Cetus.Application.CreateCategory;
using Cetus.Application.SearchAllCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command)
    {
        var created = await _mediator.Send(command);

        if (created) return Ok();

        return BadRequest();
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _mediator.Send(new SearchAllCategoriesQuery());

        return Ok(categories);
    }
}
