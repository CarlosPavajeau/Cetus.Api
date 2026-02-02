using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Application.Customers.FindByPhone;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Customers;

public class FindByPhone : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/by-phone/{phone}", async (
            string phone,
            IQueryHandler<FindCustomerByPhoneQuery, CustomerResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var query = new FindCustomerByPhoneQuery(phone);
            var result = await cache.GetOrCreateAsync(
                $"customer-by-phone-{phone}",
                async token => await handler.Handle(query, token),
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(2),
                    LocalCacheExpiration = TimeSpan.FromHours(2)
                },
                cancellationToken: cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Customers);
    }
}
