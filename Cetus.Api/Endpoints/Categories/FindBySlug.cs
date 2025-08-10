using Application.Abstractions.Messaging;
using Application.Categories.FindBySlug;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Categories;

internal sealed class FindBySlug : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories/{slug}", async (
                string slug,
                HybridCache cache,
                IQueryHandler<FindCategoryBySlugQuery, FindCategoryBySlugResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new FindCategoryBySlugQuery(slug);

                var result = await cache.GetOrCreateAsync(
                    $"categories-slug-{slug}",
                    async token => await handler.Handle(query, token),
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromHours(5),
                        LocalCacheExpiration = TimeSpan.FromHours(5)
                    },
                    cancellationToken: cancellationToken
                );

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .WithTags(Tags.Categories);
    }
}
