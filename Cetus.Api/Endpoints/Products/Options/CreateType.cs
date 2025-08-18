using Application.Abstractions.Messaging;
using Application.Products.Options.CreateType;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Products.Options;

public class CreateType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/option-types", async (
            CreateProductOptionTypeCommand command,
            ICommandHandler<CreateProductOptionTypeCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
