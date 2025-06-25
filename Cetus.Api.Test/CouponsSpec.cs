using System.Net;
using System.Net.Http.Json;
using Application.Coupons;
using Application.Coupons.Create;
using Application.Coupons.Redeem;
using Application.Orders.Create;
using Application.Orders.Find;
using Application.Products.Find;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Domain.Coupons;
using Shouldly;

namespace Cetus.Api.Test;

public class CouponsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly CreateCouponCommandFaker _couponCommandFaker = new();
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Guid _cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");

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

    [Fact(DisplayName = "Should return all rules for a coupon")]
    public async Task ShouldReturnAllRulesForACoupon()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/coupons", newCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        // Act
        var response = await Client.GetAsync($"api/coupons/{coupon.Id}/rules");

        // Assert
        response.EnsureSuccessStatusCode();

        var rules = await response.DeserializeAsync<List<CouponRuleResponse>>();
        rules.ShouldNotBeNull();
        rules.ShouldNotBeEmpty();
        rules.Count.ShouldBe(newCoupon.Rules.Count);
    }

    [Fact(DisplayName = "Should successfully redeem a valid coupon")]
    public async Task ShouldSuccessfullyRedeemAValidCoupon()
    {
        // Arrange
        var newCoupon = _couponCommandFaker
            .WithFixedAmountDiscount()
            .WithNoRules()
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", newCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder();
        var originalTotal = order.Total;

        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify order total was updated
        var updatedOrder = await GetOrder(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Total.ShouldBeLessThan(originalTotal);
    }

    [Fact(DisplayName = "Should not redeem a non-existent coupon")]
    public async Task ShouldNotRedeemANonExistentCoupon()
    {
        // Arrange
        var order = await CreateTestOrder();
        var redeemCommand = new RedeemCouponCommand("INVALID123", order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should not redeem an expired coupon")]
    public async Task ShouldNotRedeemAnExpiredCoupon()
    {
        // Arrange
        var expiredCoupon = _couponCommandFaker
            .WithNoRules()
            .RuleFor(c => c.EndDate, _ => DateTime.UtcNow.AddDays(-1))
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", expiredCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder();
        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should not redeem a coupon for non-existent order")]
    public async Task ShouldNotRedeemACouponForNonExistentOrder()
    {
        // Arrange
        var newCoupon = _couponCommandFaker.WithNoRules().Generate();
        var createResponse = await Client.PostAsJsonAsync("api/coupons", newCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var redeemCommand = new RedeemCouponCommand(coupon.Code, Guid.NewGuid());

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should not redeem a coupon that exceeds usage limit",
        Skip = "This test is currently ignored because it requires a specific setup for usage limits.")]
    public async Task ShouldNotRedeemACouponThatExceedsUsageLimit()
    {
        // Arrange
        var limitedCoupon = _couponCommandFaker
            .WithNoRules()
            .RuleFor(c => c.UsageLimit, _ => 1)
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", limitedCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order1 = await CreateTestOrder();
        var order2 = await CreateTestOrder();

        var redeemCommand1 = new RedeemCouponCommand(coupon.Code, order1.Id);
        var redeemCommand2 = new RedeemCouponCommand(coupon.Code, order2.Id);

        // Act - First redemption should succeed
        var response1 = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand1);
        response1.EnsureSuccessStatusCode();

        // Act - Second redemption should fail
        var response2 = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should not redeem a coupon when order doesn't meet minimum purchase")]
    public async Task ShouldNotRedeemACouponWhenOrderDoesntMeetMinimumPurchase()
    {
        // Arrange
        var minPurchaseCoupon = _couponCommandFaker
            .RuleFor(c => c.Rules, _ => new List<CreateCouponRuleCommand>
            {
                new(CouponRuleType.MinPurchaseAmount, "1000.00")
            })
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", minPurchaseCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder(); // Order with low total
        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should successfully redeem a coupon with percentage discount")]
    public async Task ShouldSuccessfullyRedeemACouponWithPercentageDiscount()
    {
        // Arrange
        var percentageCoupon = _couponCommandFaker
            .WithPercentageDiscount()
            .WithNoRules()
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", percentageCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder();
        var originalTotal = order.Total;
        var expectedDiscount = originalTotal * (coupon.DiscountValue / 100);

        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.EnsureSuccessStatusCode();

        var updatedOrder = await GetOrder(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Total.ShouldBe(originalTotal - expectedDiscount);
    }

    [Fact(DisplayName = "Should successfully redeem a coupon with fixed amount discount")]
    public async Task ShouldSuccessfullyRedeemACouponWithFixedAmountDiscount()
    {
        // Arrange
        var fixedAmountCoupon = _couponCommandFaker
            .WithFixedAmountDiscount()
            .WithNoRules()
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", fixedAmountCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder();
        var originalTotal = order.Total;
        var expectedDiscount = Math.Min(coupon.DiscountValue, originalTotal);

        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.EnsureSuccessStatusCode();

        var updatedOrder = await GetOrder(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.Total.ShouldBe(originalTotal - expectedDiscount);
    }

    [Fact(DisplayName = "Should successfully redeem a coupon with free shipping")]
    public async Task ShouldSuccessfullyRedeemACouponWithFreeShipping()
    {
        // Arrange
        var freeShippingCoupon = _couponCommandFaker
            .WithFreeShipping()
            .WithNoRules()
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", freeShippingCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order = await CreateTestOrder();
        var originalDeliveryFee = order.DeliveryFee;

        var redeemCommand = new RedeemCouponCommand(coupon.Code, order.Id);

        // Act
        var response = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand);

        // Assert
        response.EnsureSuccessStatusCode();

        var updatedOrder = await GetOrder(order.Id);
        updatedOrder.ShouldNotBeNull();
        updatedOrder.DeliveryFee.ShouldBe(0);
        updatedOrder.Total.ShouldBe(order.Total - originalDeliveryFee);
    }

    [Fact(DisplayName = "Should not redeem a coupon twice by the same customer",
        Skip = "This test is currently ignored because it requires a specific setup for one-per-customer rules.")]
    public async Task ShouldNotRedeemACouponTwiceByTheSameCustomer()
    {
        // Arrange
        var onePerCustomerCoupon = _couponCommandFaker
            .WithNoRules()
            .RuleFor(c => c.Rules, _ => new List<CreateCouponRuleCommand>
            {
                new(CouponRuleType.OnePerCustomer, "")
            })
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/coupons", onePerCustomerCoupon);
        createResponse.EnsureSuccessStatusCode();

        var coupon = await createResponse.DeserializeAsync<CouponResponse>();
        coupon.ShouldNotBeNull();

        var order1 = await CreateTestOrder();
        var order2 = await CreateTestOrder();

        var redeemCommand1 = new RedeemCouponCommand(coupon.Code, order1.Id);
        var redeemCommand2 = new RedeemCouponCommand(coupon.Code, order2.Id);

        // Act - First redemption should succeed
        var response1 = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand1);
        response1.EnsureSuccessStatusCode();

        // Act - Second redemption should fail
        var response2 = await Client.PostAsJsonAsync("api/coupons/redeem", redeemCommand2);

        // Assert
        response2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private async Task<OrderResponse> CreateTestOrder()
    {
        // Create a test product first
        var newProduct = _productCommandFaker.Generate();
        var createProductResponse = await Client.PostAsJsonAsync("api/products", newProduct);
        createProductResponse.EnsureSuccessStatusCode();

        var product = await createProductResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Create a test order
        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 2, product.Price, product.Id)
        };

        var newOrder = new CreateOrderCommand(
            "123 Test Street",
            _cityId,
            150.00m,
            newOrderItems,
            newCustomer
        );

        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();

        var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();
        return order;
    }

    private async Task<OrderResponse?> GetOrder(Guid orderId)
    {
        var response = await Client.GetAsync($"api/orders/{orderId}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.DeserializeAsync<OrderResponse>();
    }
}
