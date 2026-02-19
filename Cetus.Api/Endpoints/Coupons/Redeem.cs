using Application.Abstractions.Messaging;
using Application.Coupons.Redeem;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class Redeem : IEndpoint
{
    private sealed record Request(string CouponCode, Guid OrderId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("coupons/redeem", async (
            Request request,
            ICommandHandler<RedeemCouponCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RedeemCouponCommand(request.CouponCode, request.OrderId);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Coupons).AllowAnonymous();
    }
}
