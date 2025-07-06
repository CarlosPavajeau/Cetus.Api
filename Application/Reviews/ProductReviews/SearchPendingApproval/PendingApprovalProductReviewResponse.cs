using System.Linq.Expressions;
using Domain.Reviews;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

public sealed record PendingApprovalProductReviewProduct(string Name, string? ImageUrl);

public sealed record PendingApprovalProductReviewResponse(
    Guid Id,
    string Comment,
    byte Rating,
    string Customer,
    PendingApprovalProductReviewProduct Product,
    DateTime CreatedAt)
{
    public static Expression<Func<ProductReview, PendingApprovalProductReviewResponse>> Map => productReview =>
        new PendingApprovalProductReviewResponse(
            productReview.Id,
            productReview.Comment,
            productReview.Rating,
            productReview.Customer!.Name,
            new PendingApprovalProductReviewProduct(
                productReview.Product!.Name,
                productReview.Product!.ImageUrl),
            productReview.CreatedAt);
}
