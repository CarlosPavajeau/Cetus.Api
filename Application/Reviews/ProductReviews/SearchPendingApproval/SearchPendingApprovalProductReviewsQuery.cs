using Application.Abstractions.Messaging;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

public record SearchPendingApprovalProductReviewsQuery : IQuery<IEnumerable<PendingApprovalProductReviewResponse>>;
