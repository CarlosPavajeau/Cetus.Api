using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Application.Orders.Update;
using Cetus.Api.Realtime;
using Cetus.Api.Requests;
using Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using SharedKernel;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[EnableRateLimiting("fixed")]
[Route("api/[controller]")]
public class WompiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WompiController> _logger;
    private readonly IHubContext<OrdersHub, IOrdersClient> _ordersHub;
    private readonly ICommandHandler<UpdateOrderCommand, OrderResponse> handler;

    public WompiController(IConfiguration configuration, ILogger<WompiController> logger,
        IHubContext<OrdersHub, IOrdersClient> ordersHub, ICommandHandler<UpdateOrderCommand, OrderResponse> handler)
    {
        _configuration = configuration;
        _logger = logger;
        _ordersHub = ordersHub;
        this.handler = handler;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] WompiRequest request)
    {
        try
        {
            _logger.LogInformation("Received Wompi request: {@Request}", request);

            var hasValidChecksum = ValidateChecksum(request);
            if (!hasValidChecksum)
            {
                return BadRequest("Invalid checksum");
            }

            var hasValidOrderId = TryParseOrderId(request, out var orderId);
            if (!hasValidOrderId)
            {
                _logger.LogWarning("Invalid order ID received from Wompi request: {OrderId}",
                    request.Data.Transaction.Reference);
                return BadRequest($"Invalid order id: {request.Data.Transaction.Reference}");
            }

            var hasProcessedTransaction = await ProcessTransaction(request, orderId);
            if (!hasProcessedTransaction)
            {
                return BadRequest($"Failed to process transaction for order with ID {orderId}");
            }

            await NotifyOrderUpdate(orderId);

            return Ok(new {Id = orderId});
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the Wompi request");
            return BadRequest($"An error occurred while processing the Wompi request: {e.Message}");
        }
    }

    private static bool TryParseOrderId(WompiRequest request, out Guid orderId)
    {
        return Guid.TryParse(request.Data.Transaction.Reference, out orderId);
    }

    private async Task<bool> ProcessTransaction(WompiRequest request, Guid orderId)
    {
        UpdateOrderCommand command;
        Result<OrderResponse> result;
        if (request.Data.Transaction.Status != "APPROVED")
        {
            command = new UpdateOrderCommand(orderId, OrderStatus.Pending, request.Data.Transaction.Id);
            result = await handler.Handle(command, CancellationToken.None);

            return result.IsSuccess;
        }

        command = new UpdateOrderCommand(orderId, OrderStatus.Paid, request.Data.Transaction.Id);
        result = await handler.Handle(command, CancellationToken.None);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update order with ID {OrderId}", orderId);
            return false;
        }

        _logger.LogInformation("Order with ID {OrderId} updated to paid", orderId);
        return true;
    }

    private async Task NotifyOrderUpdate(Guid orderId)
    {
        await _ordersHub.Clients.Group(orderId.ToString()).ReceiveUpdatedOrder();
    }

    private bool ValidateChecksum(WompiRequest request)
    {
        var eventSecret = _configuration["Wompi:EventSecret"];
        if (string.IsNullOrEmpty(eventSecret))
        {
            _logger.LogWarning("Wompi event secret is not configured for environment: {Environment}",
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            return false;
        }

        var properties = request.Signature.Properties;

        var stringBuilder = new StringBuilder();
        foreach (var property in properties)
        {
            var propertyValue = property switch
            {
                "transaction.id" => request.Data.Transaction.Id,
                "transaction.reference" => request.Data.Transaction.Reference,
                "transaction.status" => request.Data.Transaction.Status,
                "transaction.amount_in_cents" => request.Data.Transaction.AmountInCents.ToString(CultureInfo
                    .InvariantCulture),
                _ => throw new ArgumentOutOfRangeException(nameof(request))
            };

            stringBuilder.Append(propertyValue);
        }

        stringBuilder.Append(request.Timestamp);
        stringBuilder.Append(eventSecret);

        var computedChecksum = ComputeChecksum(stringBuilder.ToString());
        var requestChecksum = request.Signature.Checksum;

        if (computedChecksum != requestChecksum)
        {
            _logger.LogWarning(
                "Invalid checksum received from Wompi request, expected: {ExpectedChecksum}, received: {ReceivedChecksum}",
                computedChecksum, requestChecksum);
        }

        return computedChecksum == requestChecksum;
    }

    private static string ComputeChecksum(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);

        var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return checksum;
    }
}
