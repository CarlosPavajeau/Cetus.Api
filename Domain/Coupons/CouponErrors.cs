using SharedKernel;

namespace Domain.Coupons;

public static class CouponErrors
{
    public static Error ConflictCodeExists(string code)
    {
        return new Error(
            "Coupon.Create.CodeExists",
            $"A coupon with the code '{code}' already exists.",
            ErrorType.Conflict
        );
    }
}
