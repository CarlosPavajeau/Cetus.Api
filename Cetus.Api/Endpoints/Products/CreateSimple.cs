using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.Create;
using Application.Products.CreateSimple;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class CreateSimple : IEndpoint
{
    private sealed record Request(
        string Name,
        string? Description,
        Guid CategoryId,
        string Sku,
        decimal Price,
        int Stock,
        IReadOnlyList<ImageRequest> Images
    );

    private sealed record ImageRequest(string ImageUrl, string? AltText, int SortOrder);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/simple", async (
            Request request,
            ICommandHandler<CreateSimpleProductCommand, ProductResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSimpleProductCommand(
                request.Name,
                request.Description,
                request.CategoryId,
                request.Sku,
                request.Price,
                request.Stock,
                [.. request.Images.Select(i => new CreateProductImage(i.ImageUrl, i.AltText, i.SortOrder))]
            );
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync(CacheKeyBuilder.Build("products", "for-sale", tenant.Id.ToString()), cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
