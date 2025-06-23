using Application.Abstractions.Messaging;

namespace Application.Coupons.Redeem;

public sealed record RedeemCouponCommand(string CouponCode, Guid OrderId) : ICommand;
