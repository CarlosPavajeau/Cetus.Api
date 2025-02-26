using Cetus.Application.CreateOrder;
using Cetus.Application.FindOrder;
using Cetus.Application.SearchAllOrders;
using Cetus.Application.UpdateOrder;
using Cetus.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new FindOrderQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var result = await _mediator.Send(new SearchAllOrdersQuery());
        return Ok(result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateOrder([FromRoute] Guid id, [FromBody] UpdateOrderCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        if (command.Status == OrderStatus.Pending)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return result is null ? NotFound() : Ok(result);
    }
}
