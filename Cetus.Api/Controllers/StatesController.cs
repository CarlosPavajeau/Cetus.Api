using Cetus.Application.SearchAllStateCities;
using Cetus.Application.SearchAllStates;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cetus.Api.Controllers;

[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class StatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetStates()
    {
        var result = await _mediator.Send(new SearchAllStatesQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}/cities")]
    public async Task<IActionResult> GetCities([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new SearchAllStateCitiesQuery(id));
        return Ok(result);
    }
}
