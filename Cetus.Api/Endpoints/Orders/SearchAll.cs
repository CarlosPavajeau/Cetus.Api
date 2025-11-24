using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            [FromServices] IQueryHandler<SearchAllOrdersQuery, PagedResult<OrderResponse>> handler,
            [AsParameters] SearchAllOrdersQuery query,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
