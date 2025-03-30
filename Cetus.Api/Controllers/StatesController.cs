using Cetus.States.Application.SearchAll;
using Cetus.States.Application.SearchAllCities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class StatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly HybridCache _cache;

    public StatesController(IMediator mediator, HybridCache cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetStates()
    {
        var result = await _cache.GetOrCreateAsync(
            "states",
            async cancellationToken => await _mediator.Send(new SearchAllStatesQuery(), cancellationToken)
        );

        return Ok(result);
    }

    [HttpGet("{id:guid}/cities")]
    public async Task<IActionResult> GetCities([FromRoute] Guid id)
    {
        var result = await _cache.GetOrCreateAsync(
            $"states-{id}-cities",
            async cancellationToken => await _mediator.Send(new SearchAllStateCitiesQuery(id), cancellationToken)
        );

        return Ok(result);
    }
}
