using Cetus.Api.Realtime;
using Cetus.Orders.Application.CalculateInsights;
using Cetus.Orders.Application.Create;
using Cetus.Orders.Application.DeliveryFees.Create;
using Cetus.Orders.Application.DeliveryFees.Find;
using Cetus.Orders.Application.DeliveryFees.SearchAll;
using Cetus.Orders.Application.Find;
using Cetus.Orders.Application.SearchAll;
using Cetus.Orders.Application.Summary;
using Cetus.Orders.Application.Update;
using Cetus.Orders.Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;

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
    private readonly HybridCache _cache;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger,
        IHubContext<OrdersHub, IOrdersClient> ordersHub, HybridCache cache)
    {
        _mediator = mediator;
        _logger = logger;
        _ordersHub = ordersHub;
        _cache = cache;
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

            if (e is ValidationException)
            {
                throw;
            }

            return BadRequest(e.Message);
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
    public async Task<IActionResult> GetOrdersInsights([FromQuery] string Month)
    {
        var result = await _cache.GetOrCreateAsync(
            $"orders-insights-{Month}",
            async cancellationToken => await _mediator.Send(new CalculateOrdersInsightsQuery(Month), cancellationToken),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            }
        );

        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetOrdersSummary([FromQuery] string Month)
    {
        var result = await _cache.GetOrCreateAsync(
            $"orders-summary-{Month}",
            async cancellationToken => await _mediator.Send(new GetOrdersSummaryQuery(Month), cancellationToken),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            }
        );

        return Ok(result);
    }
    
    [HttpPost("delivery-fees")]
    public async Task<IActionResult> CreateDeliveryFee([FromBody] CreateDeliveryFeeCommand command)
    {
        var result = await _mediator.Send(command);
        
        await _cache.RemoveAsync("delivery-fees");
        
        return Ok(result);
    }
    
    [HttpGet("delivery-fees")]
    public async Task<IActionResult> GetDeliveryFees()
    {
        var result = await _cache.GetOrCreateAsync(
            "delivery-fees",
            async cancellationToken => await _mediator.Send(new SearchAllDeliveryFeesQuery(), cancellationToken)
        );

        return Ok(result);
    }
    
    [HttpGet("delivery-fees/{cityId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDeliveryFee([FromRoute] Guid cityId)
    {
        var result = await _cache.GetOrCreateAsync(
            $"delivery-fee-{cityId}",
            async cancellationToken => await _mediator.Send(new FindDeliveryFeeQuery(cityId), cancellationToken)
        );
        
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
