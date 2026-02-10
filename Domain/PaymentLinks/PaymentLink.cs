using Domain.Orders;

namespace Domain.PaymentLinks;

public sealed class PaymentLink
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Token { get; set; } = string.Empty;
    public PaymentLinkStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order? Order { get; set; }
}
