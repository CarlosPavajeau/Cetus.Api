using System.Text.Json.Serialization;
using Application.Abstractions.Messaging;
using Application.Abstractions.Wompi;
using Application.Orders;
using Application.Orders.Pay;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Cetus.Api.Realtime;
using Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Endpoints.Webhooks;

internal sealed class WompiPaymentNotification : IEndpoint
{
    private sealed record WompiTransaction(
        string Id,
        string Reference,
        string Status,
        [property: JsonPropertyName("amount_in_cents")]
        decimal AmountInCents
    );

    private sealed record WompiData(WompiTransaction Transaction);

    private sealed record WompiSignature(IEnumerable<string> Properties, string Checksum);

    private sealed record WompiRequest(
        string Event,
        WompiData Data,
        string Environment,
        WompiSignature Signature,
        long Timestamp
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/wompi/payments", async (
            [FromBody] WompiRequest request,
            IWompiClient wompiClient,
            IHubContext<OrdersHub, IOrdersClient> ordersHub,
            ILogger<WompiPaymentNotification> logger,
            ICommandHandler<PayOrderCommand, SimpleOrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            logger.LogInformation("Received Wompi payment notification with transaction {Transaction}",
                request.Data.Transaction.Id);

            var payment = await wompiClient.FindPaymentById(request.Data.Transaction.Id, cancellationToken);

            if (payment is null)
            {
                logger.LogWarning("Payment with ID {PaymentId} not found", request.Data.Transaction.Id);
                return Results.NotFound();
            }

            bool hasValidOrderId = Guid.TryParse(request.Data.Transaction.Reference, out var orderId);
            if (!hasValidOrderId)
            {
                logger.LogWarning("Invalid order id received from Mercado Pago payment: {ExternalReference}",
                    request.Data.Transaction.Reference);
                return Results.BadRequest();
            }

            if (payment.Status != "APPROVED")
            {
                return Results.Ok(new
                {
                    Message = $"Order with id {orderId} has not been paid yet"
                });
            }

            string paymentId = payment.TransactionId;
            var payOrderCommand = new PayOrderCommand(orderId, paymentId, PaymentProvider.Wompi);

            var updateResult = await handler.Handle(payOrderCommand, cancellationToken);

            if (updateResult.IsSuccess)
            {
                await ordersHub.Clients.Group(orderId.ToString()).ReceiveUpdatedOrder();
            }

            return updateResult.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Webhooks, Tags.Wompi);
    }
}
