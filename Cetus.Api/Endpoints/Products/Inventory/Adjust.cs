using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Inventory.Adjust;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Inventory;

internal sealed class Adjust : IEndpoint
{
    private sealed record Request(
        string? GlobalReason,
        string UserId,
        IReadOnlyList<AdjustmentItemRequest> Adjustments
    );

    private sealed record AdjustmentItemRequest(long VariantId, int Value, AdjustmentType Type, string? Reason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("inventory/adjustments", async (
            Request request,
            ICommandHandler<AdjustInventoryStockCommand> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new AdjustInventoryStockCommand(
                request.GlobalReason,
                request.UserId,
                [
                    .. request.Adjustments.Select(a =>
                        new InventoryAdjustmentItem(a.VariantId, a.Value, a.Type, a.Reason))
                ]
            );
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveByTagAsync(["products", $"tenant-{tenant.Id}"], cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Inventory);
    }
}
