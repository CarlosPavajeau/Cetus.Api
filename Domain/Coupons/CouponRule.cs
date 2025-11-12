namespace Domain.Coupons;

public sealed class CouponRule
{
    public long Id { get; set; }
    public long CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public CouponRuleType RuleType { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
