using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.ChangeStatus;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class ChangeStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}/status", async (
            Guid id,
            [FromBody] ChangeOrderStatusCommand command,
            ICommandHandler<ChangeOrderStatusCommand> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            if (id != command.OrderId)
            {
                return Results.BadRequest("Mismatched order ID in URL and body.");
            }

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveByTagAsync([$"reports:t={tenant.Id}"], cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
