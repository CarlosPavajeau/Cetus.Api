using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Application.Customers.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("customers/{id:guid}", async (
            Guid id,
            UpdateCustomerCommand command,
            ICommandHandler<UpdateCustomerCommand, CustomerResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            string normalizedPhone = new([.. command.Phone.Where(char.IsDigit)]);
            var result = await handler.Handle(command with { Id = id, Phone = normalizedPhone }, cancellationToken);

            string cacheKey = $"customer-by-phone-{normalizedPhone}";
            if (result.IsSuccess)
            {
                await cache.RemoveAsync(cacheKey, cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Customers);
    }
}
