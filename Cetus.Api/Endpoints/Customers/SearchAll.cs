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
            string cacheKey = $"customers-{context.Id}-{query.Page}-{query.PageSize}-{query.Search}-{query.SortBy}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query, token),
                cancellationToken: cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Customers);
    }
}
