using Application.Abstractions.Messaging;
using Application.Orders.SearchTimeline;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchTimeline : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{orderId:guid}/timeline", async (
            Guid orderId,
            IQueryHandler<SearchOrderTimelineQuery, IEnumerable<OrderTimelineResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchOrderTimelineQuery(orderId);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
