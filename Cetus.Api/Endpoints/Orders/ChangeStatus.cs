using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.ChangeStatus;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Orders;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class ChangeStatus : IEndpoint
{
    private sealed record Request(
        OrderStatus NewStatus,
        PaymentMethod? PaymentMethod = null,
        PaymentStatus? PaymentStatus = null,
        string? UserId = null,
        string? Notes = null
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("orders/{id:guid}/status", async (
            Guid id,
            Request request,
            ICommandHandler<ChangeOrderStatusCommand> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeOrderStatusCommand(id, request.NewStatus, request.PaymentMethod, request.PaymentStatus, request.UserId, request.Notes);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveByTagAsync([$"reports:t={tenant.Id}"], cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
