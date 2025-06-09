using Domain.Orders;

namespace Domain.Reviews;

public sealed class ReviewRequest
{
    public Guid Id { get; set; }
    public ReviewRequestStatus Status { get; set; } = ReviewRequestStatus.Pending;
    public string Token { get; set; } = string.Empty;

    public Guid OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public Customer? Customer { get; set; }

    public DateTime SendAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
