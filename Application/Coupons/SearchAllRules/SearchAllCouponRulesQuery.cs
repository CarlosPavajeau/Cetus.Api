using Application.Abstractions.Messaging;

namespace Application.Coupons.SearchAllRules;

public sealed record SearchAllCouponRulesQuery(long Id) : IQuery<IEnumerable<CouponRuleResponse>>;
