using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Coupons;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Coupons.Redeem;

internal sealed class RedeemCouponCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RedeemCouponCommand>
{
    public async Task<Result> Handle(RedeemCouponCommand command, CancellationToken cancellationToken)
    {
        var coupon = await context.Coupons
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Code == command.CouponCode, cancellationToken);

        if (coupon == null)
        {
            return Result.Failure(CouponErrors.NotFound);
        }

        var currentDate = dateTimeProvider.UtcNow;
        if (!coupon.IsValidForUsage(currentDate))
        {
            return Result.Failure(CouponErrors.InvalidOrExpired);
        }

        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        var alreadyUsed = await context.CouponUsages
            .AnyAsync(u => u.CouponId == coupon.Id && u.OrderId == order.Id, cancellationToken);
        if (alreadyUsed)
        {
            return Result.Failure(CouponErrors.AlreadyUsed);
        }

        var validationResult = await ValidateCouponRules(coupon, order, cancellationToken);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var discountAmount = CalculateDiscountAmount(coupon, order);
        if (discountAmount < 0)
        {
            return Result.Failure(CouponErrors.NoDiscountApplicable);
        }

        if (coupon.DiscountType == CouponDiscountType.FreeShipping)
        {
            order.DeliveryFee = 0;
        }

        var couponUsage = new CouponUsage
        {
            CouponId = coupon.Id,
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            DiscountAmountApplied = discountAmount
        };

        order.Total -= discountAmount;

        context.CouponUsages.Add(couponUsage);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ValidateCouponRules(Coupon coupon, Order order, CancellationToken cancellationToken)
    {
        foreach (var rule in coupon.Rules)
        {
            var ruleValidation = rule.RuleType switch
            {
                CouponRuleType.MinPurchaseAmount => ValidateMinPurchaseAmount(rule, order),
                CouponRuleType.SpecificProduct => ValidateSpecificProduct(rule, order),
                CouponRuleType.SpecificCategory => ValidateSpecificCategory(rule, order),
                CouponRuleType.OnePerCustomer => await ValidateOnePerCustomer(coupon, order, cancellationToken),
                _ => Result.Failure(CouponErrors.InvalidRule)
            };

            if (ruleValidation.IsFailure)
            {
                return ruleValidation;
            }
        }

        return Result.Success();
    }

    private static Result ValidateMinPurchaseAmount(CouponRule rule, Order order)
    {
        if (!decimal.TryParse(rule.Value, out var minAmount))
        {
            return Result.Failure(CouponErrors.InvalidRule);
        }

        var orderSubtotal = order.Total;
        if (orderSubtotal < minAmount)
        {
            return Result.Failure(CouponErrors.MinimumPurchaseNotMet);
        }

        return Result.Success();
    }

    private static Result ValidateSpecificProduct(CouponRule rule, Order order)
    {
        if (!Guid.TryParse(rule.Value, out var productId))
        {
            return Result.Failure(CouponErrors.InvalidRule);
        }

        var hasProduct = order.Items.Any(i => i.ProductId == productId);
        if (!hasProduct)
        {
            return Result.Failure(CouponErrors.ProductNotInOrder);
        }


        return Result.Success();
    }

    private static Result ValidateSpecificCategory(CouponRule rule, Order order)
    {
        if (!Guid.TryParse(rule.Value, out var categoryId))
        {
            return Result.Failure(CouponErrors.InvalidRule);
        }

        var hasCategory = order.Items.Any(i => i.Product?.CategoryId == categoryId);
        if (!hasCategory)
        {
            return Result.Failure(CouponErrors.CategoryNotInOrder);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateOnePerCustomer(Coupon coupon, Order order, CancellationToken cancellationToken)
    {
        var existingUsage = await context.CouponUsages
            .AnyAsync(u => u.CouponId == coupon.Id && u.CustomerId == order.CustomerId, cancellationToken);

        if (existingUsage)
        {
            return Result.Failure(CouponErrors.AlreadyUsedByCustomer);
        }

        return Result.Success();
    }

    private static decimal CalculateDiscountAmount(Coupon coupon, Order order)
    {
        var orderSubtotal = order.Total;

        return coupon.DiscountType switch
        {
            CouponDiscountType.Percentage => orderSubtotal * (coupon.DiscountValue / 100),
            CouponDiscountType.FixedAmount => Math.Min(coupon.DiscountValue, orderSubtotal),
            _ => 0
        };
    }
}
