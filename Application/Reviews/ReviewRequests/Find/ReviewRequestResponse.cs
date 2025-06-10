using Domain.Reviews;

namespace Application.Reviews.ReviewRequests.Find;

public sealed record ReviewRequestProduct(
    string Name,
    string ImageUrl);

public sealed record ReviewRequestResponse(
    Guid Id,
    ReviewRequestStatus Status,
    string Customer,
    ReviewRequestProduct Product)
{
    public static ReviewRequestResponse FromReviewRequest(ReviewRequest reviewRequest)
    {
        return new ReviewRequestResponse(
            reviewRequest.Id,
            reviewRequest.Status,
            reviewRequest.Customer?.Name ?? string.Empty,
            new ReviewRequestProduct(
                reviewRequest.OrderItem?.ProductName ?? string.Empty,
                reviewRequest.OrderItem?.ImageUrl ?? string.Empty));
    }
}
