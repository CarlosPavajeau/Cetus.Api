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

    public bool IsExpired
    {
        get
        {
            if (EndDate.HasValue && EndDate.Value < DateTime.UtcNow)
            {
                return true;
            }

            return StartDate.HasValue && StartDate.Value > DateTime.UtcNow;
        }
    }

    public bool IsValidForUsage => IsActive && !IsExpired && (UsageLimit == null || UsageCount < UsageLimit);
}
