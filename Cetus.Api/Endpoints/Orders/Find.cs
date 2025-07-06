using Application.Abstractions.Messaging;
using Application.Orders.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}", async (
            Guid id,
            IQueryHandler<FindOrderQuery, OrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindOrderQuery(id);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
