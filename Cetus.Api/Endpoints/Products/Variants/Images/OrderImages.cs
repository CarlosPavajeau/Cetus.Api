using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.Variants.Images.Order;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Variants.Images;

internal sealed class OrderImages : IEndpoint
{
    private sealed record Request(IReadOnlyList<ImageRequest> Images);

    private sealed record ImageRequest(long Id, string ImageUrl, string? AltText, int SortOrder);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/variants/{id:long}/images/order", async (
            long id,
            Request request,
            ICommandHandler<OrderVariantImagesCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new OrderVariantImagesCommand(
                id,
                [.. request.Images.Select(i => new ProductImageResponse(i.Id, i.ImageUrl, i.AltText, i.SortOrder))]
            );
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
