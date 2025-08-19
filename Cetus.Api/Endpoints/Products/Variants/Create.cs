using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Variants.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Variants;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/{productId:guid}/variants", async (
            Guid productId,
            CreateProductVariantCommand command,
            ICommandHandler<CreateProductVariantCommand> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            if (productId != command.ProductId)
            {
                return Results.BadRequest("Product ID mismatch.");
            }

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync($"product-variants-{tenant.Id}-{productId}", cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
