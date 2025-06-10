using Domain.Reviews;

namespace Application.Reviews.ProductReviews.SearchAll;

public sealed record ProductReviewResponse(Guid Id, string Comment, byte Rating, string Customer, DateTime CreatedAt)
{
    public static ProductReviewResponse FromProductReview(ProductReview productReview)
    {
        return new ProductReviewResponse(
            productReview.Id,
            productReview.Comment,
            productReview.Rating,
            productReview.Customer?.Name ?? "Desconocido",
            productReview.CreatedAt);
    }
}
