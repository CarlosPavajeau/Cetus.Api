using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class Create : IEndpoint
{
    private sealed record Request(Guid ReviewRequestId, string Comment, byte Rating);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews/products", async (
            Request request,
            ICommandHandler<CreateProductReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductReviewCommand(request.ReviewRequestId, request.Comment, request.Rating);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Reviews).AllowAnonymous();
    }
}
