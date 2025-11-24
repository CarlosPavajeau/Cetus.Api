using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            IQueryHandler<SearchAllOrdersQuery, PagedResult<OrderResponse>> handler,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] OrderStatus[]? statuses = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            CancellationToken cancellationToken = default) =>
        {
            var query = new SearchAllOrdersQuery(
                page,
                pageSize,
                statuses,
                from,
                to
            );

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
