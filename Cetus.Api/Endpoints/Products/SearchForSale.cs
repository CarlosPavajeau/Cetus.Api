using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.SearchForSale;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Cetus.Api.Endpoints.Products;

internal sealed class SearchForSale : IEndpoint
{
    private sealed record Request(
        int Page = 1,
        int PageSize = 20,
        Guid[]? CategoryIds = null,
        string? SearchTerm = null
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("products/for-sale", async (
            [AsParameters] Request request,
            [FromServices]
            IQueryHandler<SearchAllProductsForSaleQuery, PagedResult<SimpleProductForSaleResponse>> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllProductsForSaleQuery(request.Page, request.PageSize, request.CategoryIds,
                request.SearchTerm);
            var result = await cache.GetOrCreateAsync(
                $"products-for-sale-${tenant.Id}-{query.Page}-{query.PageSize}-{string.Join(",", query.CategoryIds ?? [])}-{query.SearchTerm}",
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Products);
    }
}
