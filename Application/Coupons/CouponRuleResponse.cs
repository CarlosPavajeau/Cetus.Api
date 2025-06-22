using Domain.Coupons;

namespace Application.Coupons;

public sealed record CouponRuleResponse(long Id, CouponRuleType RuleType, string Value)
{
    public static CouponRuleResponse FromCouponRule(CouponRule couponRule)
    {
        return new CouponRuleResponse(couponRule.Id, couponRule.RuleType, couponRule.Value);
    }
}
