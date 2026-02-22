using Application.Abstractions.Messaging;
using Application.PaymentProviders.Wompi;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class IntegritySignature : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{orderId:guid}/payment-providers/wompi/integrity-signature", async (
            Guid orderId,
            IQueryHandler<GenerateWompiIntegritySignatureQuery, string> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GenerateWompiIntegritySignatureQuery(orderId);

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
