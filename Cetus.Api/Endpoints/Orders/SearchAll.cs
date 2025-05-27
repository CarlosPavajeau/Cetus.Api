using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            IQueryHandler<SearchAllOrdersQuery, IEnumerable<OrderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllOrdersQuery();
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
