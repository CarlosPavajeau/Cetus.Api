using System.Net.Http.Json;
using Application.Orders.Create;
using Application.Reports.DailySummary;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
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
}
