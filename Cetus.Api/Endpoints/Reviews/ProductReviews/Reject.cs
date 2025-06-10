using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.Reject;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class Reject : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("reviews/products/{id:guid}/reject", async (
            Guid id,
            [FromBody] RejectProductReviewCommand command,
            ICommandHandler<RejectProductReviewCommand> handler,
            CancellationToken cancellationToken) =>
        {
            if (command.Id != id)
            {
                return Results.BadRequest("The review ID in the URL does not match the ID in the request body.");
            }
            
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Reviews).RequireAuthorization();
    }
} 
