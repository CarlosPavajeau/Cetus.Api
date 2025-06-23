using Application.Abstractions.Messaging;
using Application.Coupons.Redeem;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class Redeem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("coupons/redeem", async (
            [FromBody] RedeemCouponCommand command,
            ICommandHandler<RedeemCouponCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        });
    }
}
