using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Variants;
using Application.Products.Variants.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Variants;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/variants/{id:long}", async (
            long id,
            UpdateProductVariantCommand command,
            ICommandHandler<UpdateProductVariantCommand, SimpleProductVariantResponse> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            if (id != command.Id)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["id"] = ["Route 'id' must match body 'id'."]
                });
            }

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                // Invalidate cache for the product variants of the associated product
                var productVariant = result.Value;
                await cache.RemoveAsync($"product-variants-{tenant.Id}-{productVariant.ProductId}", cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
