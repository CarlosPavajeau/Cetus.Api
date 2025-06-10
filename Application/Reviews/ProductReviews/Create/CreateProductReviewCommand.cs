using Application.Abstractions.Messaging;

namespace Application.Reviews.ProductReviews.Create;

public sealed record CreateProductReviewCommand(Guid ReviewRequestId, string Comment, byte Rating) : ICommand;
