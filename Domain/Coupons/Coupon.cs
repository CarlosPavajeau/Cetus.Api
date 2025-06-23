namespace Domain.Coupons;

public sealed class Coupon
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int UsageCount { get; set; }
    public int? UsageLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<CouponRule> Rules { get; set; } = new List<CouponRule>();
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired(DateTime currentDate)
    {
        if (EndDate.HasValue && EndDate.Value < currentDate)
        {
            return true;
        }

        return StartDate.HasValue && StartDate.Value > currentDate;
    }

    public bool IsValidForUsage(DateTime currentDate)
    {
        return IsActive && !IsExpired(currentDate) && (UsageLimit == null || UsageCount < UsageLimit);
    }
}
