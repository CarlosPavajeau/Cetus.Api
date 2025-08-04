using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.SearchForSale;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchForSale : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/for-sale", async (
            IQueryHandler<SearchAllProductsForSaleQuery, IEnumerable<SimpleProductForSaleResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductsForSaleQuery();

            var result = await cache.GetOrCreateAsync(
                $"products-for-sale-${tenant.Id}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
