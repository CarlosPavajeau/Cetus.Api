using System.Net.Http.Json;
using Cetus.Api.Test.Shared;
using Cetus.Application.CreateOrder;
using Cetus.Application.CreateProduct;
using Cetus.Application.FindOrder;
using Cetus.Application.SearchAllProducts;
using Cetus.Domain;
using Shouldly;

namespace Cetus.Api.Test;

public class OrdersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should create a new order")]
    public async Task ShouldCreateANewOrder()
    {
        // Arrange 
        var newProduct =
            new CreateProductCommand("test-find", null, 1500, 10, Guid.NewGuid());
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = new CreateOrderCustomer("test-id", "test-name", "test-email", "test-phone", "test-address");
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, 1, product.Price, product.Id)
        };

        var newOrder = new CreateOrderCommand("test-address", product.Price, newOrderItems, newCustomer);

        // Act
        var response =
            await Client.PostAsJsonAsync("api/orders", newOrder);

        // Assert
        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<Guid>();

        order.ShouldNotBe(Guid.Empty);
    }
    
    [Fact(DisplayName = "Should get an order")]
    public async Task ShouldGetAnOrder()
    {
        // Arrange
        var newProduct =
            new CreateProductCommand("test-find", null, 1500, 10, Guid.NewGuid());
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = new CreateOrderCustomer("test-id", "test-name", "test-email", "test-phone", "test-address");
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, 1, product.Price, product.Id)
        };

        var newOrder = new CreateOrderCommand("test-address", product.Price, newOrderItems, newCustomer);

        var response =
            await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<Guid>();

        // Act
        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId}");

        // Assert
        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId);
        orderResponse.Status.ShouldBe(OrderStatus.Pending);
    }
}
