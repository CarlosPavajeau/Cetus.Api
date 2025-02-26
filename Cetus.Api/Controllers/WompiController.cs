using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Cetus.Api.Requests;
using Cetus.Application.FindOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Controllers;

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
    public async Task<IActionResult> Post([FromBody] WompiRequest request)
    {
        try
        {
            var hasValidChecksum = ValidateChecksum(request);
            if (!hasValidChecksum)
            {
                return BadRequest();
            }

            if (!Guid.TryParse(request.Data.Transaction.Reference, out var orderId))
            {
                return BadRequest();
            }
            
            var order = await _mediator.Send(new FindOrderQuery(orderId));
            
            if (order == null)
            {
                return NotFound();
            }
            
            // TODO: Implement the logic to update the order status based on the Wompi transaction status

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

        return computedChecksum == requestChecksum;
    }

    private static string ComputeChecksum(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        var checksum = Convert.ToBase64String(hash);
        return checksum;
    }
}
