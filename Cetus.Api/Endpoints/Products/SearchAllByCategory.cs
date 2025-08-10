using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.SearchAllByCategory;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchAllByCategory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/category/{categoryId:guid}", async (
            Guid categoryId,
            HybridCache cache,
            IQueryHandler<SearchAllProductsByCategory, IEnumerable<SimpleProductForSaleResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductsByCategory(categoryId);

            var result = await cache.GetOrCreateAsync(
                $"products-category-{categoryId}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
