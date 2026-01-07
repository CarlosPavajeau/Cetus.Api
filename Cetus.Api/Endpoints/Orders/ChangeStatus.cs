using Application.Abstractions.Messaging;
using Application.Orders.ChangeStatus;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class ChangeStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}/status", async (
            Guid id,
            [FromBody] ChangeOrderStatusCommand command,
            ICommandHandler<ChangeOrderStatusCommand> handler,
            CancellationToken cancellationToken) =>
        {
            if (id != command.OrderId)
            {
                return Results.BadRequest("Mismatched order ID in URL and body.");
            }

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
