using Application.Abstractions.Messaging;
using Application.PaymentLinks;
using Application.PaymentLinks.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.PaymentLinks;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{orderId:guid}/payment-link", async (
            Guid orderId,
            ICommandHandler<CreatePaymentLinkCommand, PaymentLinkResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new CreatePaymentLinkCommand(orderId);

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.PaymentLinks);
    }
}
