using Application.Abstractions.Messaging;
using Application.Reviews.ReviewRequests.Find;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ReviewRequests;

internal sealed class Find : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reviews/requests/{token}", async (
            string token,
            IQueryHandler<FindReviewRequestQuery, ReviewRequestResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new FindReviewRequestQuery(token);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reviews).AllowAnonymous();
    }
}
