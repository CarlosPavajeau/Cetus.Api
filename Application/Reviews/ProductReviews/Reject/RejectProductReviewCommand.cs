using Application.Abstractions.Messaging;

namespace Application.Reviews.ProductReviews.Reject;

public sealed record RejectProductReviewCommand(Guid Id, string? ModeratorNotes) : ICommand;
