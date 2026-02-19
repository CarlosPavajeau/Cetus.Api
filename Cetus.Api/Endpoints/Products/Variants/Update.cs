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
    private sealed record Request(
        decimal Price,
        bool Enabled,
        bool Featured,
        decimal? CostPrice = null,
        decimal? CompareAtPrice = null
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/variants/{id:long}", async (
            long id,
            Request request,
            ICommandHandler<UpdateProductVariantCommand, SimpleProductVariantResponse> handler,
            ITenantContext tenant,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductVariantCommand(id, request.Price, request.Enabled, request.Featured,
                request.CostPrice, request.CompareAtPrice);
            var result = await handler.Handle(command, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Match(Results.Ok, CustomResults.Problem);
            }

            var productVariant = result.Value;
            await cache.RemoveAsync(
                CacheKeyBuilder.Build("products", "variants", tenant.Id.ToString(),
                    productVariant.ProductId.ToString()), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
