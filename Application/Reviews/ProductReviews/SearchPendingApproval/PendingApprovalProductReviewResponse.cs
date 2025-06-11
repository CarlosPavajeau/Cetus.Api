using Domain.Reviews;

namespace Application.Reviews.ProductReviews.SearchPendingApproval;

public sealed record PendingApprovalProductReviewProduct(string Name, string ImageUrl);

public sealed record PendingApprovalProductReviewResponse(
    Guid Id,
    string Comment,
    byte Rating,
    string Customer,
    PendingApprovalProductReviewProduct Product,
    DateTime CreatedAt)
{
    public static PendingApprovalProductReviewResponse FromProductReview(ProductReview productReview)
    {
        return new PendingApprovalProductReviewResponse(
            productReview.Id,
            productReview.Comment,
            productReview.Rating,
            productReview.Customer?.Name ?? "Desconocido",
            new PendingApprovalProductReviewProduct(
                productReview.Product?.Name ?? "Desconocido",
                productReview.Product?.ImageUrl ?? string.Empty),
            productReview.CreatedAt);
    }
}
