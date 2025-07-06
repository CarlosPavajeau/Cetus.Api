using System.Linq.Expressions;
using Domain.Reviews;

namespace Application.Reviews.ProductReviews.SearchAll;

public sealed record ProductReviewResponse(Guid Id, string Comment, byte Rating, string Customer, DateTime CreatedAt)
{
    public static Expression<Func<ProductReview, ProductReviewResponse>> Map => review =>
        new ProductReviewResponse(
            review.Id,
            review.Comment,
            review.Rating,
            review.Customer!.Name,
            review.CreatedAt);
}
