using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Coupons;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Coupons.Redeem;

internal sealed class RedeemCouponCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenant,
    IDateTimeProvider dateTimeProvider,
    ILogger<RedeemCouponCommandHandler> logger)
    : ICommandHandler<RedeemCouponCommand>
{
    public async Task<Result> Handle(RedeemCouponCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting coupon redemption. Coupon: {@Coupon}", command);

        var coupon = await context.Coupons
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Code == command.CouponCode && c.StoreId == tenant.Id, cancellationToken);

        if (coupon == null)
        {
            logger.LogWarning("Coupon not found. CouponCode: {CouponCode}", command.CouponCode);
            return Result.Failure(CouponErrors.NotFound(command.CouponCode));
        }

        var currentDate = dateTimeProvider.UtcNow;
        if (!coupon.IsValidForUsage(currentDate))
        {
            logger.LogWarning("Coupon is invalid or expired. CouponCode: {CouponCode}, CurrentDate: {CurrentDate}",
                command.CouponCode, currentDate);
            return Result.Failure(CouponErrors.InvalidOrExpired);
        }

        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

        if (order == null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", command.OrderId);
            return Result.Failure(OrderErrors.NotFound(command.OrderId));
        }

        bool alreadyUsed = await context.CouponUsages
            .AnyAsync(u => u.CouponId == coupon.Id && u.OrderId == order.Id, cancellationToken);
        if (alreadyUsed)
        {
            logger.LogWarning("Coupon already used for this order. CouponId: {CouponId}, OrderId: {OrderId}", coupon.Id,
                order.Id);
            return Result.Failure(CouponErrors.AlreadyUsed);
        }

        var validationResult = await ValidateCouponRules(coupon, order, cancellationToken);
        if (validationResult.IsFailure)
        {
            logger.LogWarning("Coupon rule validation failed. CouponId: {CouponId}, OrderId: {OrderId}, Error: {Error}",
                coupon.Id, order.Id, validationResult.Error);
            return validationResult;
        }

        decimal discountAmount = CalculateDiscountAmount(coupon, order);
        if (discountAmount < 0)
        {
            logger.LogWarning("No discount applicable. CouponId: {CouponId}, OrderId: {OrderId}", coupon.Id, order.Id);
            return Result.Failure(CouponErrors.NoDiscountApplicable);
        }

        if (coupon.DiscountType == CouponDiscountType.FreeShipping)
        {
            logger.LogInformation("Applying free shipping. OrderId: {OrderId}", order.Id);
            order.DeliveryFee = 0;
        }

        var couponUsage = new CouponUsage
        {
            CouponId = coupon.Id,
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            DiscountAmountApplied = discountAmount
        };

        order.Discount += discountAmount;
        order.Total -= discountAmount;

        logger.LogInformation("Discount applied. New order total: {OrderTotal}", order.Total);

        context.CouponUsages.Add(couponUsage);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result> ValidateCouponRules(Coupon coupon, Order order, CancellationToken cancellationToken)
    {
        foreach (var rule in coupon.Rules)
        {
            Result ruleValidation;
            switch (rule.RuleType)
            {
                case CouponRuleType.MinPurchaseAmount:
                    ruleValidation = ValidateMinPurchaseAmount(rule, order);
                    break;
                case CouponRuleType.SpecificProduct:
                    ruleValidation = ValidateSpecificProduct(rule, order);
                    break;
                case CouponRuleType.SpecificCategory:
                    ruleValidation = ValidateSpecificCategory(rule, order);
                    break;
                case CouponRuleType.OnePerCustomer:
                    ruleValidation = await ValidateOnePerCustomer(coupon, order, cancellationToken);
                    break;
                default:
                    logger.LogWarning("Unknown coupon rule type. RuleType: {RuleType}, CouponId: {CouponId}",
                        rule.RuleType, coupon.Id);
                    ruleValidation = Result.Failure(CouponErrors.InvalidRule);
                    break;
            }

            if (ruleValidation.IsFailure)
            {
                logger.LogWarning(
                    "Coupon rule validation failed. RuleType: {RuleType}, RuleValue: {RuleValue}, Error: {Error}",
                    rule.RuleType, rule.Value, ruleValidation.Error);
                return ruleValidation;
            }
        }

        return Result.Success();
    }

    private static Result ValidateMinPurchaseAmount(CouponRule rule, Order order)
    {
        if (!decimal.TryParse(rule.Value, out decimal minAmount))
        {
            return Result.Failure(CouponErrors.InvalidRule);
        }

        decimal orderSubtotal = order.Total;
        if (orderSubtotal < minAmount)
        {
            return Result.Failure(CouponErrors.MinimumPurchaseNotMet);
        }

        return Result.Success();
    }

    private static Result ValidateSpecificProduct(CouponRule rule, Order order)
    {
        if (!long.TryParse(rule.Value, out long variantId))
        {
            return Result.Failure(CouponErrors.InvalidRule);
        }

        bool hasProduct = order.Items.Any(i => i.VariantId == variantId);
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

        bool hasCategory = order.Items.Any(i => i.Product?.CategoryId == categoryId);
        if (!hasCategory)
        {
            return Result.Failure(CouponErrors.CategoryNotInOrder);
        }

        return Result.Success();
    }

    private async Task<Result> ValidateOnePerCustomer(Coupon coupon, Order order, CancellationToken cancellationToken)
    {
        bool existingUsage = await context.CouponUsages
            .AnyAsync(u => u.CouponId == coupon.Id && u.CustomerId == order.CustomerId, cancellationToken);

        if (existingUsage)
        {
            logger.LogWarning("Coupon already used by this customer. CouponId: {CouponId}, CustomerId: {CustomerId}",
                coupon.Id, order.CustomerId);
            return Result.Failure(CouponErrors.AlreadyUsedByCustomer);
        }

        return Result.Success();
    }

    private static decimal CalculateDiscountAmount(Coupon coupon, Order order)
    {
        decimal orderSubtotal = order.Total;

        return coupon.DiscountType switch
        {
            CouponDiscountType.Percentage =>
                Math.Round(orderSubtotal * (coupon.DiscountValue / 100), 2, MidpointRounding.ToEven),
            CouponDiscountType.FixedAmount =>
                Math.Round(Math.Min(coupon.DiscountValue, orderSubtotal), 2, MidpointRounding.ToEven),
            _ => 0
        };
    }
}
