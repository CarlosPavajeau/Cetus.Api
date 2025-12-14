using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Inventory.Adjust;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Inventory;

internal sealed class Adjust : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("inventory/adjustments", async (
            AdjustInventoryStockCommand command,
            ICommandHandler<AdjustInventoryStockCommand> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveByTagAsync(["products", $"tenant-{tenant.Id}"], cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Inventory);
    }
}
