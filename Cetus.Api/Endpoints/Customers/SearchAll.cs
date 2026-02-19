using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers", async (
            HybridCache cache,
            [FromServices] IQueryHandler<SearchAllCustomersQuery, PagedResult<CustomerSummaryResponse>> handler,
            [AsParameters] SearchAllCustomersQuery query,
            ITenantContext context,
            CancellationToken cancellationToken) =>
        {
            var queryParams = new List<KeyValuePair<string, string>>
            {
                new("page", query.Page.ToString()),
                new("pageSize", query.PageSize.ToString()),
                new("search", query.Search ?? ""),
                new("sortBy", query.SortBy?.ToString() ?? ""),
            };
            string cacheKey = CacheKeyBuilder.BuildWithQuery("customers", queryParams, context.Id.ToString());

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Customers);
    }
}
