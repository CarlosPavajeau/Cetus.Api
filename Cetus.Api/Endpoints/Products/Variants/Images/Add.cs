using Application.Abstractions.Messaging;
using Application.Products.Variants.Images.Add;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Variants.Images;

internal sealed class Add : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/variants/{id:long}/images", async (
            long id,
            AddVariantImagesCommand command,
            ICommandHandler<AddVariantImagesCommand> handler,
            CancellationToken cancellationToken) =>
        {
            if (id != command.Id)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["id"] = ["Route 'id' must match body 'Id'."]
                });
            }

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
