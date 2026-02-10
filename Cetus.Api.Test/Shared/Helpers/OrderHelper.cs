using System.Net.Http.Json;
using Application.Orders;
using Application.Orders.Create;
using Bogus;
using Cetus.Api.Test.Shared.Fakers;
using Shouldly;

namespace Cetus.Api.Test.Shared.Helpers;

public static class OrderHelper
{
    private static readonly Faker _faker = new();
    private static readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private static readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");

    public static async Task<SimpleOrderResponse> CreateOrder(HttpClient client, long variantId)
    {
        var shippingInfo = new CreateOrderShipping(
            _faker.Address.FullAddress(),
            cityId
        );

        var command = new CreateOrderCommand(
            [new CreateOrderItem(variantId, 1)],
            _orderCustomerFaker.Generate(),
            shippingInfo
        );

        var response = await client.PostAsJsonAsync("api/orders", command);

        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<SimpleOrderResponse>();

        order.ShouldNotBeNull();

        return order;
    }
}
