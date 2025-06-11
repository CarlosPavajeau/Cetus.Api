using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.SearchPendingApproval;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Reviews.ProductReviews;

internal sealed class SearchPendingApproval : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("reviews/products/pending", async (
            IQueryHandler<SearchPendingApprovalProductReviewsQuery, IEnumerable<PendingApprovalProductReviewResponse>>
                handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchPendingApprovalProductReviewsQuery();
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Reviews).RequireAuthorization();
    }
}
