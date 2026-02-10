using Application.Abstractions.Messaging;
using Application.PaymentLinks;
using Application.PaymentLinks.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.PaymentLinks;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("payment-links/{token}", async (
            string token,
            IQueryHandler<FindPaymentLinkQuery, PaymentLinkResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindPaymentLinkQuery(token);

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.PaymentLinks);
    }
}
