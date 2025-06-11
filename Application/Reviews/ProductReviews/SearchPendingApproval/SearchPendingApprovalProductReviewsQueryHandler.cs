using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

internal sealed class SearchPendingApprovalProductReviewsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchPendingApprovalProductReviewsQuery, IEnumerable<PendingApprovalProductReviewResponse>>
{
    public async Task<Result<IEnumerable<PendingApprovalProductReviewResponse>>> Handle(
        SearchPendingApprovalProductReviewsQuery query,
        CancellationToken cancellationToken)
    {
        var pendingReviews = await context.ProductReviews
            .Include(pr => pr.Customer)
            .Include(pr => pr.Product)
            .Where(r => r.Status == ProductReviewStatus.PendingApproval)
            .ToListAsync(cancellationToken);

        var responses = pendingReviews.Select(PendingApprovalProductReviewResponse.FromProductReview).ToList();

        return responses;
    }
}
