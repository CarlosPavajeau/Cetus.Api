using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.SearchAllOrders;
using Application.Orders;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class SearchAllOrders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/{customerId:guid}/orders", async (
            Guid customerId,
            [AsParameters] SearchAllCustomerOrdersQuery query,
            IQueryHandler<SearchAllCustomerOrdersQuery, PagedResult<SimpleOrderResponse>> handler,
            HybridCache cache,
            ITenantContext context,
            CancellationToken cancellationToken) =>
        {
            string cacheKey =
                $"customer:{customerId}:orders:t={context.Id}:p={query.Page}:ps={query.PageSize}";

            var result = await cache.GetOrCreateAsync(
                cacheKey,
                async token => await handler.Handle(query with { CustomerId = customerId }, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                cancellationToken: cancellationToken
            );

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Customers);
    }
}
