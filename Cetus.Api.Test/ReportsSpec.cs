using System.Globalization;
using System.Net.Http.Json;
using Application.Orders;
using Application.Orders.Create;
using Application.Reports.DailySummary;
using Application.Reports.MonthlyProfitability;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Orders;
using Shouldly;

namespace Cetus.Api.Test;

public class ReportsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Faker _faker = new();

    private CreateOrderCommand GenerateCreateOrderCommand(CreateProductWithVariantResponse product, int quantity = 1)
    {
        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.VariantId, quantity)
        };
        var shippingInfo = new CreateOrderShipping(
            _faker.Address.FullAddress(),
            cityId
        );

        var newOrder = new CreateOrderCommand(
            newOrderItems,
            newCustomer,
            shippingInfo
        );
        return newOrder;
    }

    [Fact(DisplayName = "Should get daily summary report")]
    public async Task ShouldGetDailySummaryReport()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var createResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("api/reports/daily-summary");
        response.EnsureSuccessStatusCode();
        var report = await response.DeserializeAsync<DailySummaryResponse>();

        // Assert
        report.ShouldNotBeNull();
        report.Orders.Total.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Should get monthly profitability report")]
    public async Task ShouldGetMonthlyProfitabilityReport()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client, 50.0m);
        var newOrder = GenerateCreateOrderCommand(product);

        var createResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createResponse.EnsureSuccessStatusCode();

        var createdOrder = await createResponse.DeserializeAsync<SimpleOrderResponse>();
        createdOrder.ShouldNotBeNull();

        await OrderHelper.ChangeStatusThrough(
            Client,
            createdOrder.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        // Act
        var response = await Client.GetAsync("api/reports/monthly-profitability");
        response.EnsureSuccessStatusCode();
        var report = await response.DeserializeAsync<MonthlyProfitabilityResponse>();

        // Assert
        report.ShouldNotBeNull();
        report.Summary.TotalSales.ShouldBeGreaterThan(0);
        report.Summary.GrossProfit.ShouldBeLessThanOrEqualTo(report.Summary.TotalSales);
        report.Trend.ShouldNotBeNull();
        report.ProductsWithoutCost.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should get monthly profitability report with date range")]
    public async Task ShouldGetMonthlyProfitabilityReportWithDateRange()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client, 50.0m);
        var newOrder = GenerateCreateOrderCommand(product);

        var createResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createResponse.EnsureSuccessStatusCode();

        var createdOrder = await createResponse.DeserializeAsync<SimpleOrderResponse>();
        createdOrder.ShouldNotBeNull();

        await OrderHelper.ChangeStatusThrough(
            Client,
            createdOrder.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        string from = DateTime.UtcNow.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string to = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // Act
        var response = await Client.GetAsync($"api/reports/monthly-profitability?from={from}&to={to}");
        response.EnsureSuccessStatusCode();
        var report = await response.DeserializeAsync<MonthlyProfitabilityResponse>();

        // Assert
        report.ShouldNotBeNull();
        report.Summary.ShouldNotBeNull();
        report.Summary.TotalSales.ShouldBeGreaterThan(0);
    }

    [Fact(DisplayName = "Should return products without cost in monthly profitability report")]
    public async Task ShouldReturnProductsWithoutCostInMonthlyProfitabilityReport()
    {
        // Arrange - CreateProductWithVariant creates variants without CostPrice
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var createResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createResponse.EnsureSuccessStatusCode();
        
        var createdOrder = await createResponse.DeserializeAsync<SimpleOrderResponse>();
        createdOrder.ShouldNotBeNull();
        
        await OrderHelper.ChangeStatusThrough(
            Client,
            createdOrder.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        // Act
        var response = await Client.GetAsync("api/reports/monthly-profitability");
        response.EnsureSuccessStatusCode();
        var report = await response.DeserializeAsync<MonthlyProfitabilityResponse>();

        // Assert
        report.ShouldNotBeNull();
        report.ProductsWithoutCost.ShouldNotBeEmpty();
    }
}
