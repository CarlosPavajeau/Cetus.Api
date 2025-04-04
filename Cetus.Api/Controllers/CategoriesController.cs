using Cetus.Categories.Application.Create;
using Cetus.Categories.Application.Delete;
using Cetus.Categories.Application.SearchAll;
using Cetus.Categories.Application.Update;
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
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly HybridCache _cache;

    public CategoriesController(IMediator mediator, HybridCache cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var created = await _mediator.Send(command);

        if (!created) return BadRequest();

        await _cache.RemoveAsync("categories");

        return Ok();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _cache.GetOrCreateAsync(
            "categories",
            async cancellationToken => await _mediator.Send(new SearchAllCategoriesQuery(), cancellationToken)
        );

        return Ok(categories);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest();

        var updated = await _mediator.Send(command);

        if (!updated) return BadRequest();

        await _cache.RemoveAsync("categories");

        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var deleted = await _mediator.Send(new DeleteCategoryCommand(id));

        if (!deleted) return BadRequest();

        await _cache.RemoveAsync("categories");

        return Ok();
    }
}
