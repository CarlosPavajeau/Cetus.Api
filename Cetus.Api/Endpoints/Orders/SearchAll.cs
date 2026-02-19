using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchAll : IEndpoint
{
    private sealed record Request(
        int Page = 1,
        int PageSize = 20,
        string[]? Statuses = null,
        DateTime? From = null,
        DateTime? To = null
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders", async (
            [AsParameters] Request request,
            [FromServices] IQueryHandler<SearchAllOrdersQuery, PagedResult<OrderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchAllOrdersQuery(
                request.Page,
                request.PageSize,
                request.Statuses,
                request.From,
                request.To
            );
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
