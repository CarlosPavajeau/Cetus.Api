using System.Globalization;
using Application.Abstractions.Messaging;
using Application.Coupons;
using Application.Coupons.SearchAllRules;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Coupons;

internal sealed class SearchAllCouponRules : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("coupons/{id:long}/rules", async (
            long id,
            HybridCache cache,
            IQueryHandler<SearchAllCouponRulesQuery, IEnumerable<CouponRuleResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllCouponRulesQuery(id);

            string cacheKey = CacheKeyBuilder.Build(
                "coupons",
                id.ToString(CultureInfo.InvariantCulture),
                "rules"
            );

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Coupons);
    }
}
