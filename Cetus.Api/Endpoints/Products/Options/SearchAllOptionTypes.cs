using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options;
using Application.Products.Options.SearchAllTypes;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Options;

internal sealed class SearchAllOptionTypes : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/option-types", async (
            IQueryHandler<SearchAllProductOptionTypesQuery, IEnumerable<ProductOptionTypeResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductOptionTypesQuery();

            var result = await cache.GetOrCreateAsync(
                $"product-option-types-{tenant.Id}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
