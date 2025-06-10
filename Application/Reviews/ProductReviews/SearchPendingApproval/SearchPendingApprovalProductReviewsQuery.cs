using Application.Abstractions.Messaging;
using Application.Reviews.ProductReviews.SearchAll;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

public record SearchPendingApprovalProductReviewsQuery : IQuery<IEnumerable<ProductReviewResponse>>;
