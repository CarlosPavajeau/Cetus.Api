using System.Net;
using System.Net.Http.Json;
using Application.Coupons;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Domain.Coupons;
using Shouldly;

namespace Cetus.Api.Test;

public class CouponsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly CreateCouponCommandFaker _couponCommandFaker = new();

    [Fact(DisplayName = "Should create a new coupon")]
    public async Task ShouldCreateANewCoupon()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.Code.ShouldBe(newCoupon.Code);
        coupon.Description.ShouldBe(newCoupon.Description);
        coupon.DiscountType.ShouldBe(newCoupon.DiscountType);
        coupon.DiscountValue.ShouldBe(newCoupon.DiscountValue);
        coupon.UsageLimit.ShouldBe(newCoupon.UsageLimit);
        coupon.StartDate.ShouldBe(newCoupon.StartDate);
        coupon.EndDate.ShouldBe(newCoupon.EndDate);
        coupon.UsageCount.ShouldBe(0);
        coupon.IsActive.ShouldBeTrue();
    }

    [Fact(DisplayName = "Should not create a coupon with duplicate code")]
    public async Task ShouldNotCreateACouponWithDuplicateCode()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/coupons", newCoupon);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Should create a coupon with percentage discount")]
    public async Task ShouldCreateACouponWithPercentageDiscount()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithPercentageDiscount().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.DiscountType.ShouldBe(CouponDiscountType.Percentage);
        coupon.DiscountValue.ShouldBeGreaterThan(0);
        coupon.DiscountValue.ShouldBeLessThanOrEqualTo(100);
    }

    [Fact(DisplayName = "Should create a coupon with fixed amount discount")]
    public async Task ShouldCreateACouponWithFixedAmountDiscount()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithFixedAmountDiscount().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.DiscountType.ShouldBe(CouponDiscountType.FixedAmount);
        coupon.DiscountValue.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Should create a coupon with free shipping")]
    public async Task ShouldCreateACouponWithFreeShipping()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithFreeShipping().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.DiscountType.ShouldBe(CouponDiscountType.FreeShipping);
        coupon.DiscountValue.ShouldBe(0);
    }

    [Fact(DisplayName = "Should create a coupon without rules")]
    public async Task ShouldCreateACouponWithoutRules()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithNoRules().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should create a coupon without dates")]
    public async Task ShouldCreateACouponWithoutDates()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithNoDates().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.StartDate.ShouldBeNull();
        coupon.EndDate.ShouldBeNull();
    }

    [Fact(DisplayName = "Should create a coupon without usage limit")]
    public async Task ShouldCreateACouponWithoutUsageLimit()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithNoUsageLimit().Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons", newCoupon);

        // Assert
        response.EnsureSuccessStatusCode();

        var coupon = await response.DeserializeAsync<CouponResponse>();

        coupon.ShouldNotBeNull();
        coupon.UsageLimit.ShouldBeNull();
    }

    [Fact(DisplayName = "Should return all coupons")]
    public async Task ShouldReturnAllCoupons()
    {
        // Arrange
        var coupon1 = _couponCommandFaker.Generate();
        var coupon2 = _couponCommandFaker.Generate();
        
        await Client.PostAsJsonAsync("api/coupons", coupon1);
        await Client.PostAsJsonAsync("api/coupons", coupon2);

        // Act
        var response = await Client.GetAsync("api/coupons");

        // Assert
        response.EnsureSuccessStatusCode();

        var coupons = await response.DeserializeAsync<List<CouponResponse>>();
        coupons.ShouldNotBeNull();
        coupons.ShouldNotBeEmpty();
        coupons.Count.ShouldBeGreaterThanOrEqualTo(2);
    }
}
