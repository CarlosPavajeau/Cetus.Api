using System.Net;
using System.Net.Http.Json;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Orders.Application.CalculateInsights;
using Cetus.Orders.Application.Create;
using Cetus.Orders.Application.Find;
using Cetus.Orders.Domain;
using Cetus.Products.Application.SearchAll;
using Shouldly;

namespace Cetus.Api.Test;

public class OrdersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Faker _faker = new();

    [Fact(DisplayName = "Should create a new order")]
    public async Task ShouldCreateANewOrder()
    {
        // Arrange 
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
            newCustomer);

        // Act
        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        // Assert
        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<Guid>();

        order.ShouldNotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Should not create a new order with invalid product stock")]
    public async Task ShouldNotCreateANewOrderWithInvalidProduct()
    {
        // Arrange
        var newProduct = _productCommandFaker.WithStock(1).Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 10, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        // Act
        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should get an order")]
    public async Task ShouldGetAnOrder()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

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

    [Fact(DisplayName = "Should get all orders")]
    public async Task ShouldGetAllOrders()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        // Act
        var getOrdersResponse = await Client.GetAsync("api/orders");

        // Assert
        getOrdersResponse.EnsureSuccessStatusCode();

        var orders = await getOrdersResponse.DeserializeAsync<IEnumerable<OrderResponse>>();

        var orderResponses = orders?.ToList();
        orderResponses.ShouldNotBeNull();
        orderResponses.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should deliver an order")]
    public async Task ShouldDeliverAnOrder()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<Guid>();

        // Act
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{orderId}/deliver", null);

        // Assert
        deliverOrderResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId);
        orderResponse.Status.ShouldBe(OrderStatus.Delivered);
    }

    [Fact(DisplayName = "Should cancel an order")]
    public async Task ShouldCancelAnOrder()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<Guid>();

        // Act
        var cancelOrderResponse = await Client.PostAsync($"api/orders/{orderId}/cancel", null);

        // Assert
        cancelOrderResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId);
        orderResponse.Status.ShouldBe(OrderStatus.Canceled);
    }

    [Fact(DisplayName = "Should get orders insights")]
    public async Task ShouldGetOrdersInsights()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };

        var newOrder =
            new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<Guid>();

        var deliverOrderResponse = await Client.PostAsync($"api/orders/{orderId}/deliver", null);

        deliverOrderResponse.EnsureSuccessStatusCode();

        // Act
        var getOrdersInsightsResponse = await Client.GetAsync("api/orders/insights");

        // Assert
        getOrdersInsightsResponse.EnsureSuccessStatusCode();

        var ordersInsights = await getOrdersInsightsResponse.DeserializeAsync<OrdersInsightsResponse>();

        ordersInsights.ShouldNotBeNull();
        ordersInsights.CurrentMonthTotal.ShouldBeGreaterThan(0);
    }
}
