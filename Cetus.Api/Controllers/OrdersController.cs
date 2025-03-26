using Cetus.Api.Realtime;
using Cetus.Orders.Application.CalculateInsights;
using Cetus.Orders.Application.Create;
using Cetus.Orders.Application.Find;
using Cetus.Orders.Application.SearchAll;
using Cetus.Orders.Application.Update;
using Cetus.Orders.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;
    private readonly IHubContext<OrdersHub, IOrdersClient> _ordersHub;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger,
        IHubContext<OrdersHub, IOrdersClient> ordersHub)
    {
        _mediator = mediator;
        _logger = logger;
        _ordersHub = ordersHub;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            await _ordersHub.Clients.All.ReceiveCreatedOrder();

            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating an order.");
            throw;
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
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
    
    [HttpGet("insights")]
    public async Task<IActionResult> GetOrdersInsights()
    {
        var result = await _mediator.Send(new CalculateOrdersInsightsQuery());
        return Ok(result);
    }
    
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> DeliverOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(id, OrderStatus.Delivered));
        return result is null ? NotFound() : Ok(result);
    }
    
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(id, OrderStatus.Canceled));
        return result is null ? NotFound() : Ok(result);
    }
}
