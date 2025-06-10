using Application.Abstractions.Messaging;

namespace Application.Reviews.ProductReviews.Approve;

public sealed record ApproveProductReviewCommand(Guid Id) : ICommand;
