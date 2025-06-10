using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.SearchAll;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

internal sealed class SearchPendingApprovalProductReviewsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchPendingApprovalProductReviewsQuery, IEnumerable<ProductReviewResponse>>
{
    public async Task<Result<IEnumerable<ProductReviewResponse>>> Handle(SearchPendingApprovalProductReviewsQuery query,
        CancellationToken cancellationToken)
    {
        var pendingReviews = await context.ProductReviews
            .Where(r => r.Status == ProductReviewStatus.PendingApproval)
            .ToListAsync(cancellationToken);

        var responses = pendingReviews.Select(ProductReviewResponse.FromProductReview).ToList();

        return responses;
    }
}
