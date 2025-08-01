using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Coupons;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Coupons.Create;

internal sealed class CreateCouponCommandHandler(IApplicationDbContext context, ITenantContext tenant)
    : ICommandHandler<CreateCouponCommand, CouponResponse>
{
    public async Task<Result<CouponResponse>> Handle(CreateCouponCommand command, CancellationToken cancellationToken)
    {
        // Check if coupon code already exists (case-insensitive)
        var couponCode = command.Code.Trim().ToUpperInvariant();
        var codeExists = await context.Coupons
            .AnyAsync(c => c.Code == couponCode, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<CouponResponse>(CouponErrors.ConflictCodeExists(command.Code));
        }

        var coupon = new Coupon
        {
            Code = couponCode,
            Description = command.Description,
            DiscountType = command.DiscountType,
            DiscountValue = command.DiscountValue,
            UsageLimit = command.UsageLimit,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Rules = command.Rules.Select(r => new CouponRule
            {
                RuleType = r.RuleType,
                Value = r.Value
            }).ToList(),
            StoreId = tenant.Id
        };

        context.Coupons.Add(coupon);
        await context.SaveChangesAsync(cancellationToken);

        return CouponResponse.FromCoupon(coupon);
    }
}
