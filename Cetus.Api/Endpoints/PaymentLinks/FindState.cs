using Application.Abstractions.Messaging;
using Application.PaymentLinks.FindState;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.PaymentLinks;

internal sealed class FindState : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{orderId:guid}/payment-link", async (
            Guid orderId,
            IQueryHandler<FindPaymentLinkStateQuery, PaymentLinkStateResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindPaymentLinkStateQuery(orderId);

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.PaymentLinks);
    }
}
