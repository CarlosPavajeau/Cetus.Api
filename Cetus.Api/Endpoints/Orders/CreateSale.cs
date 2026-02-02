using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.CreateSale;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class CreateSale : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sales", async (
            [FromBody] CreateSaleCommand command,
            ICommandHandler<CreateSaleCommand, SimpleOrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
