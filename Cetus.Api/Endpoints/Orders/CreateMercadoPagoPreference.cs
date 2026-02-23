using Application.Abstractions.Messaging;
using Application.PaymentProviders.MercadoPago;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class CreateMercadoPagoPreference : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{orderId:guid}/payment-providers/mercado-pago/preference", async (
            Guid orderId,
            ICommandHandler<CreateMercadoPagoPreferenceCommand, string> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateMercadoPagoPreferenceCommand(orderId);

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
