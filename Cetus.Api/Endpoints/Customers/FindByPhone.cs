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
            string normalizedPhone = new([.. phone.Where(char.IsDigit)]);
            string cacheKey = $"customer-by-phone-{normalizedPhone}";
            var query = new FindCustomerByPhoneQuery(normalizedPhone);

            var result = await cache.GetOrCreateAsync(
                cacheKey,
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
