using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Cetus.Api.Requests;
using Cetus.Application.ApproveOrder;
using Cetus.Application.FindOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WompiController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<WompiController> _logger;

    public WompiController(IConfiguration configuration, IMediator mediator, ILogger<WompiController> logger)
    {
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] WompiRequest request)
    {
        try
        {
            _logger.LogInformation("Received Wompi request: {Request}", request);
            
            var hasValidChecksum = ValidateChecksum(request);
            if (!hasValidChecksum)
            {
                _logger.LogWarning("Invalid checksum received from Wompi request");
                return BadRequest();
            }

            if (!Guid.TryParse(request.Data.Transaction.Reference, out var orderId))
            {
                _logger.LogWarning("Invalid order ID received from Wompi request");
                return BadRequest();
            }
            
            var order = await _mediator.Send(new FindOrderQuery(orderId));
            
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                return NotFound();
            }

            if (request.Data.Transaction.Status != "APPROVED")
            {
                return Ok();
            }

            var orderApproved = await _mediator.Send(new ApproveOrderCommand(orderId));
            if (!orderApproved)
            {
                _logger.LogWarning("Failed to approve order with ID {OrderId}", orderId);
                return BadRequest();
            }
                
            _logger.LogInformation("Order with ID {OrderId} approved", orderId);
            return Ok();

        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the Wompi request");
            return BadRequest();
        }
    }

    private bool ValidateChecksum(WompiRequest request)
    {
        var eventSecret = _configuration["Wompi:EventSecret"];
        if (string.IsNullOrEmpty(eventSecret))
        {
            _logger.LogWarning("Wompi event secret is not configured");
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
        
        _logger.LogInformation("Computed checksum: {ComputedChecksum}", computedChecksum);
        _logger.LogInformation("Request checksum: {RequestChecksum}", requestChecksum);

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
