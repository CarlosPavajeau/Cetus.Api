using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Coupons;
using Application.Coupons.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("coupons", async (
            IQueryHandler<SearchAllCouponsQuery, IEnumerable<CouponResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllCouponsQuery();

            var result = await cache.GetOrCreateAsync(
                $"coupons-{tenant.Id}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Coupons).HasPermission(ClerkPermissions.AppAccess);
    }
}
