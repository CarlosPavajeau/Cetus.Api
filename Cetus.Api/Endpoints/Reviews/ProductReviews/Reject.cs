using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.Reject;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class Reject : IEndpoint
{
    private sealed record Request(string? ModeratorNotes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews/products/{id:guid}/reject", async (
            Guid id,
            Request request,
            ICommandHandler<RejectProductReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RejectProductReviewCommand(id, request.ModeratorNotes);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Reviews).RequireAuthorization();
    }
}
