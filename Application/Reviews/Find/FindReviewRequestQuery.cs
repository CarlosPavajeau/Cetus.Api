using Application.Abstractions.Messaging;

namespace Application.Reviews.Find;

public record FindReviewRequestQuery(string Token) : IQuery<ReviewRequestResponse>;
