using System.Linq.Expressions;
using Domain.Coupons;

namespace Application.Coupons;

public sealed record CouponRuleResponse(long Id, CouponRuleType RuleType, string Value)
{
    public static Expression<Func<CouponRule, CouponRuleResponse>> Map => couponRule =>
        new CouponRuleResponse(couponRule.Id, couponRule.RuleType, couponRule.Value);
}
