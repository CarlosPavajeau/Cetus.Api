using Application.Abstractions.Messaging;
using Application.Orders.SearchPayment;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class SearchPayment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("orders/{id:guid}/payments", async (
            [FromRoute] Guid id,
            IQueryHandler<SearchOrderPaymentQuery, OrderPaymentResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchOrderPaymentQuery(id);

            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
