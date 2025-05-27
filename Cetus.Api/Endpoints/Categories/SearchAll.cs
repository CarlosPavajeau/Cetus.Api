using Application.Abstractions.Messaging;
using Application.Categories.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Categories;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories", async (
                HybridCache cache,
                IQueryHandler<SearchAllCategoriesQuery, IEnumerable<CategoryResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new SearchAllCategoriesQuery();

                var result = await cache.GetOrCreateAsync(
                    "categories",
                    async token => await handler.Handle(query, token),
                    cancellationToken: cancellationToken
                );

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .AllowAnonymous()
            .WithTags(Tags.Categories);
    }
}
