using System.Linq.Expressions;
using Domain.Coupons;

namespace Application.Coupons;

public sealed record CouponResponse(
    long Id,
    string Code,
    string? Description,
    CouponDiscountType DiscountType,
    decimal DiscountValue,
    int UsageCount,
    int? UsageLimit,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static Expression<Func<Coupon, CouponResponse>> Map => coupon =>
        new CouponResponse(
            coupon.Id,
            coupon.Code,
            coupon.Description,
            coupon.DiscountType,
            coupon.DiscountValue,
            coupon.UsageCount,
            coupon.UsageLimit,
            coupon.IsActive,
            coupon.StartDate,
            coupon.EndDate,
            coupon.CreatedAt,
            coupon.UpdatedAt
        );

    public static CouponResponse FromCoupon(Coupon coupon) => Map.Compile()(coupon);
}
