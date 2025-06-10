using Application.Abstractions.Messaging;

namespace Application.Reviews.ReviewRequests.Find;

public record FindReviewRequestQuery(string Token) : IQuery<ReviewRequestResponse>;
