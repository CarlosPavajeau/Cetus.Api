using System.Globalization;
using Application.Coupons.Create;
using Bogus;
using Domain.Coupons;

namespace Cetus.Api.Test.Shared.Fakers;

public sealed class CreateCouponCommandFaker : Faker<CreateCouponCommand>
{
    public CreateCouponCommandFaker()
    {
        CustomInstantiator(faker => new CreateCouponCommand(
            faker.Commerce.Ean8().ToUpper(),
            faker.Commerce.ProductDescription(),
            faker.PickRandom<CouponDiscountType>(),
            faker.Random.Decimal(1, 100),
            faker.Random.Int(1, 1000),
            faker.Date.Recent(),
            faker.Date.Future(),
            new List<CreateCouponRuleCommand>
            {
                new(CouponRuleType.MinPurchaseAmount, faker.Random.Decimal(10, 100).ToString(CultureInfo.InvariantCulture)),
                new(CouponRuleType.SpecificCategory, faker.Commerce.Categories(1)[0])
            }
        ));
    }

    public CreateCouponCommandFaker WithPercentageDiscount()
    {
        RuleFor(c => c.DiscountType, _ => CouponDiscountType.Percentage);
        RuleFor(c => c.DiscountValue, f => f.Random.Decimal(1, 100));
        return this;
    }

    public CreateCouponCommandFaker WithFixedAmountDiscount()
    {
        RuleFor(c => c.DiscountType, _ => CouponDiscountType.FixedAmount);
        RuleFor(c => c.DiscountValue, f => f.Random.Decimal(1, 100));
        return this;
    }

    public CreateCouponCommandFaker WithFreeShipping()
    {
        RuleFor(c => c.DiscountType, _ => CouponDiscountType.FreeShipping);
        RuleFor(c => c.DiscountValue, _ => 0);
        return this;
    }

    public CreateCouponCommandFaker WithNoRules()
    {
        RuleFor(c => c.Rules, _ => new List<CreateCouponRuleCommand>());
        return this;
    }

    public CreateCouponCommandFaker WithNoDates()
    {
        RuleFor(c => c.StartDate, _ => null);
        RuleFor(c => c.EndDate, _ => null);
        return this;
    }

    public CreateCouponCommandFaker WithNoUsageLimit()
    {
        RuleFor(c => c.UsageLimit, _ => null);
        return this;
    }
}
