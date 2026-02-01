using Domain.Orders;
using Domain.Products;

namespace Domain.Reviews;

public sealed class ProductReview
{
    public Guid Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public byte Rating { get; set; }
    public bool IsVerified { get; set; }
    public ProductReviewStatus Status { get; set; } = ProductReviewStatus.PendingApproval;
    public string? ModeratorNotes { get; set; }

    public Guid ReviewRequestId { get; set; }
    public ReviewRequest? ReviewRequest { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void Reject(string? commandModeratorNotes)
    {
        Status = ProductReviewStatus.Rejected;
        ModeratorNotes = commandModeratorNotes;
    }

    public void Approve()
    {
        Status = ProductReviewStatus.Approved;
    }
}
