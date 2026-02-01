namespace Domain.Coupons;

public sealed class CouponUsage
{
    public long Id { get; set; }
    public long CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public decimal DiscountAmountApplied { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
