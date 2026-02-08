using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customers/{id:guid}", async (
            Guid id,
            IQueryHandler<FindCustomerQuery, CustomerResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindCustomerQuery(id);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Customers);
    }
}
