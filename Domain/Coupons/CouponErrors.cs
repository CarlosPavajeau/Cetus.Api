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

    public static Error NotFound => new(
        "Coupon.NotFound",
        "Coupon not found.",
        ErrorType.NotFound
    );

    public static Error InvalidOrExpired => new(
        "Coupon.InvalidOrExpired",
        "Coupon is invalid or expired.",
        ErrorType.Validation
    );

    public static Error InvalidRule => new(
        "Coupon.InvalidRule",
        "Coupon rule is invalid.",
        ErrorType.Validation
    );

    public static Error MinimumPurchaseNotMet => new(
        "Coupon.MinimumPurchaseNotMet",
        "Order does not meet the minimum purchase amount requirement.",
        ErrorType.Validation
    );

    public static Error ProductNotInOrder => new(
        "Coupon.ProductNotInOrder",
        "Required product is not in the order.",
        ErrorType.Validation
    );

    public static Error CategoryNotInOrder => new(
        "Coupon.CategoryNotInOrder",
        "Required category is not in the order.",
        ErrorType.Validation
    );

    public static Error AlreadyUsedByCustomer => new(
        "Coupon.AlreadyUsedByCustomer",
        "Coupon has already been used by this customer.",
        ErrorType.Validation
    );

    public static Error NoDiscountApplicable => new(
        "Coupon.NoDiscountApplicable",
        "No discount can be applied to this order.",
        ErrorType.Validation
    );
}
