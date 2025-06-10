using Application.Abstractions.Messaging;

namespace Application.Reviews.ProductReviews.SearchAll;

public sealed record SearchAllProductReviewsQuery(Guid ProductId) : IQuery<IEnumerable<ProductReviewResponse>>;
