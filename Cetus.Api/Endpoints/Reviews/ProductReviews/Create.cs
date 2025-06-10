using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews/products", async (
            CreateProductReviewCommand command,
            ICommandHandler<CreateProductReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Reviews).AllowAnonymous();
    }
}
