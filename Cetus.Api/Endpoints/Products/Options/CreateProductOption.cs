using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Options;

internal sealed class CreateProductOption : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/{productId:guid}/options", async (
            Guid productId,
            CreateProductOptionCommand command,
            ICommandHandler<CreateProductOptionCommand> handler,
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
                await cache.RemoveAsync($"product-options-{tenant.Id}-{productId}", cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
