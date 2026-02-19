using Application.Abstractions.Messaging;
using Application.Products.Create;
using Application.Products.Variants.Images.Add;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Variants.Images;

internal sealed class Add : IEndpoint
{
    private sealed record Request(IReadOnlyCollection<ImageRequest> Images);

    private sealed record ImageRequest(string ImageUrl, string? AltText, int SortOrder);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/variants/{id:long}/images", async (
            long id,
            Request request,
            ICommandHandler<AddVariantImagesCommand, AddVariantImagesCommandResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddVariantImagesCommand(
                id,
                [.. request.Images.Select(i => new CreateProductImage(i.ImageUrl, i.AltText, i.SortOrder))]
            );
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
