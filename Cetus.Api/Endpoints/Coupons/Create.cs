using Application.Abstractions.Messaging;
using Application.Coupons;
using Application.Coupons.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("coupons", async (
            [FromBody] CreateCouponCommand command,
            ICommandHandler<CreateCouponCommand, CouponResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Coupons).HasPermission(ClerkPermissions.AppAccess);
    }
} 
