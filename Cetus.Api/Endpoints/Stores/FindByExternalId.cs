using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.FindByExternalId;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class FindByExternalId : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stores/by-external-id/{externalId}", async (
            string externalId,
            IQueryHandler<FindStoreByExternalId, StoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindStoreByExternalId(externalId);
            string cacheKey = $"store:by-external-id:${externalId}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(15),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Stores);
    }
}
