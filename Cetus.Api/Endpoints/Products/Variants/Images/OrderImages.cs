using Application.Abstractions.Messaging;
using Application.Products.Variants.Images.Order;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Variants.Images;

internal sealed class OrderImages : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/variants/{id:long}/images/order", async (
            long id,
            OrderVariantImagesCommand command,
            ICommandHandler<OrderVariantImagesCommand> handler,
            CancellationToken cancellationToken) =>
        {
            if (id != command.VariantId)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["id"] = ["Route 'id' must match body 'VariantId'."]
                });
            }

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
