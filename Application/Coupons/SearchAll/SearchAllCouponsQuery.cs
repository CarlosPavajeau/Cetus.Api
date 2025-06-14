using Application.Abstractions.Messaging;

namespace Application.Coupons.SearchAll;

public sealed record SearchAllCouponsQuery : IQuery<IEnumerable<CouponResponse>>;
