using Application.Abstractions.Messaging;
using Application.Coupons;
using Application.Coupons.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("coupons", async (
            IQueryHandler<SearchAllCouponsQuery, IEnumerable<CouponResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllCouponsQuery();
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Coupons).HasPermission(ClerkPermissions.AppAccess);
    }
}
