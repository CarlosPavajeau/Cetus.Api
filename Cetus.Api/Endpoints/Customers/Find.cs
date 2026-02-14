using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Cetus.Api.Infrastructure;
using Domain.Customers;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/{id:guid}", async (
            Guid id,
            IQueryHandler<FindCustomerQuery, CustomerResponse> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            string cacheKey = $"customer:{id}:t={tenant.Id}";
            var query = new FindCustomerQuery(id);

            var cached = await cache.GetOrCreateAsync<CustomerResponse?>(
                cacheKey,
                async token =>
                {
                    var result = await handler.Handle(query, token);
                    return result.IsSuccess ? result.Value : null;
                },
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5),
                    LocalCacheExpiration = TimeSpan.FromMinutes(5)
                },
                cancellationToken: cancellationToken);

            return cached is not null
                ? Results.Ok(cached)
                : CustomResults.Problem(Result.Failure(CustomerErrors.NotFound(id)));
        }).WithTags(Tags.Customers);
    }
}
