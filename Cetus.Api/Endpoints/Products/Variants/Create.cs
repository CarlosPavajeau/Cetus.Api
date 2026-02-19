using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Create;
using Application.Products.Variants;
using Application.Products.Variants.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Variants;

internal sealed class Create : IEndpoint
{
    private sealed record Request(
        string Sku,
        decimal Price,
        int Stock,
        IReadOnlyList<long> OptionValueIds,
        IReadOnlyList<ImageRequest> Images,
        decimal? CostPrice = null,
        decimal? CompareAtPrice = null
    );

    private sealed record ImageRequest(string ImageUrl, string? AltText, int SortOrder);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/{productId:guid}/variants", async (
            Guid productId,
            Request request,
            ICommandHandler<CreateProductVariantCommand, SimpleProductVariantResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductVariantCommand(
                productId,
                request.Sku,
                request.Price,
                request.Stock,
                request.OptionValueIds,
                [.. request.Images.Select(i => new CreateProductImage(i.ImageUrl, i.AltText, i.SortOrder))],
                request.CostPrice,
                request.CompareAtPrice
            );
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Match(Results.Ok, CustomResults.Problem);
            }

            string cacheKey = CacheKeyBuilder.Build(
                "products",
                "variants",
                tenant.Id.ToString(),
                productId.ToString()
            );
            await cache.RemoveAsync(cacheKey, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
