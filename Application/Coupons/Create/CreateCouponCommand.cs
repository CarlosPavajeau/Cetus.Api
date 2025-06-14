using Application.Abstractions.Messaging;
using Domain.Coupons;

namespace Application.Coupons.Create;

public sealed record CreateCouponCommand(
    string Code,
    string? Description,
    CouponDiscountType DiscountType,
    decimal DiscountValue,
    int? UsageLimit,
    DateTime? StartDate,
    DateTime? EndDate,
    IReadOnlyList<CreateCouponRuleCommand> Rules
) : ICommand<CouponResponse>;

public sealed record CreateCouponRuleCommand(
    CouponRuleType RuleType,
    string Value
);
