using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Application.Orders.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Cetus.Api.Realtime;
using Domain.Orders;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Endpoints.Webhooks;

internal sealed class MercadoPagoPaymentNotification : IEndpoint
{
    sealed record MercadoPagoPaymentData(long Id);

    sealed record MercadoPagoPaymentNotificationRequest(MercadoPagoPaymentData Data);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/mercado-pago/payments", async (
            MercadoPagoPaymentNotificationRequest request,
            IMercadoPagoClient mercadoPagoClient,
            IHubContext<OrdersHub, IOrdersClient> ordersHub,
            ILogger<MercadoPagoPaymentNotification> logger,
            ICommandHandler<UpdateOrderCommand, OrderResponse> handler,
            CancellationToken cancellationToken
        ) =>
        {
            logger.LogInformation("Received Mercado Pago payment notification: {@Request}", request);

            var payment = await mercadoPagoClient.FindPaymentById(request.Data.Id, cancellationToken);

            if (payment is null)
            {
                logger.LogWarning("Payment with ID {PaymentId} not found", request.Data.Id);
                return Results.NotFound();
            }

            var hasValidOrderId = Guid.TryParse(payment.ExternalReference, out var orderId);
            if (!hasValidOrderId)
            {
                logger.LogWarning("Invalid order id received from Mercado Pago payment: {ExternalReference}",
                    payment.ExternalReference);
                return Results.BadRequest();
            }

            if (payment.Status != "approved")
            {
                return Results.Ok(new
                {
                    Message = $"Order with id {orderId} has not been paid yet"
                });
            }

            var updateOrderCommand = new UpdateOrderCommand(orderId, OrderStatus.Paid, payment.Id.ToString());
            var updateResult = await handler.Handle(updateOrderCommand, cancellationToken);

            if (updateResult.IsSuccess)
            {
                await ordersHub.Clients.Group(orderId.ToString()).ReceiveUpdatedOrder();
            }

            return updateResult.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous();
    }
}
