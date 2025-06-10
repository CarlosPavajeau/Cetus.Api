using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.Approve;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class Approve : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews/products/{id:guid}/approve", async (
            Guid id,
            ICommandHandler<ApproveProductReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ApproveProductReviewCommand(id);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Reviews).RequireAuthorization();
    }
} 
