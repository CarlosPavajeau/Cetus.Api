using Application.Abstractions.Messaging;
using Application.Products.Variants.Images.Delete;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Products.Variants.Images;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("products/variants/images/{imageId:long}", async (
            long imageId,
            [FromQuery] long variantId,
            ICommandHandler<DeleteVariantImageCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteVariantImageCommand(variantId, imageId);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
