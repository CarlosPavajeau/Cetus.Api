using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Orders.CalculateInsights;
using Application.Orders.Create;
using Application.Orders.DeliveryFees.Create;
using Application.Orders.DeliveryFees.Find;
using Application.Orders.Find;
using Application.Orders.Summary;
using Application.Products.SearchAll;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Domain.Orders;
using Domain.States;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class OrdersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private const decimal DeliveryFee = 100m;
    
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

        var order = await response.DeserializeAsync<OrderResponse>();

        order.ShouldNotBeNull();
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

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId.Id}");

        // Assert
        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId.Id);
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

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{orderId.Id}/deliver", null);

        // Assert
        deliverOrderResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId.Id}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId.Id);
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

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var cancelOrderResponse = await Client.PostAsync($"api/orders/{orderId.Id}/cancel", null);

        // Assert
        cancelOrderResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId.Id}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId.Id);
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

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        var deliverOrderResponse = await Client.PostAsync($"api/orders/{orderId.Id}/deliver", null);

        deliverOrderResponse.EnsureSuccessStatusCode();

        // Act
        var month = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
        var getOrdersInsightsResponse = await Client.GetAsync($"api/orders/insights?month={month}");

        // Assert
        getOrdersInsightsResponse.EnsureSuccessStatusCode();

        var ordersInsights = await getOrdersInsightsResponse.DeserializeAsync<OrdersInsightsResponse>();

        ordersInsights.ShouldNotBeNull();
        ordersInsights.CurrentMonthTotal.ShouldBeGreaterThan(0);
        ordersInsights.RevenuePercentageChange.ShouldBeGreaterThanOrEqualTo(0);
        ordersInsights.OrdersCountPercentageChange.ShouldBeGreaterThanOrEqualTo(0);
    }
    
    [Fact(DisplayName = "Should get orders summary")]
    public async Task ShouldGetOrdersSummary()
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

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        var deliverOrderResponse = await Client.PostAsync($"api/orders/{orderId.Id}/deliver", null);

        deliverOrderResponse.EnsureSuccessStatusCode();

        // Act
        var month = DateTime.Now.ToString("MMMM", CultureInfo.InvariantCulture);
        var getOrdersSummaryResponse = await Client.GetAsync($"api/orders/summary?month={month}");

        // Assert
        getOrdersSummaryResponse.EnsureSuccessStatusCode();

        var ordersSummary = await getOrdersSummaryResponse.DeserializeAsync<IEnumerable<OrderSummaryResponse>>();

        ordersSummary.ShouldNotBeNull().ShouldNotBeEmpty();
    }
    
    [Fact(DisplayName = "Should create a new delivery fee")]
    public async Task ShouldCreateANewDeliveryFee()
    {
        // Arrange
        var deliveryFeeCommand = new CreateDeliveryFeeCommand(cityId, 100);

        // Act
        var createDeliveryFeeResponse = await Client.PostAsJsonAsync("api/orders/delivery-fees", deliveryFeeCommand);

        // Assert
        createDeliveryFeeResponse.EnsureSuccessStatusCode();

        var deliveryFee = await createDeliveryFeeResponse.DeserializeAsync<DeliveryFeeResponse>();

        deliveryFee.ShouldNotBeNull();
        deliveryFee.Fee.ShouldBe(deliveryFeeCommand.Fee);
    }
    
    [Fact(DisplayName = "Should get all delivery fees")]
    public async Task ShouldGetAllDeliveryFees()
    {
        // Arrange
        var city = new City
        {
            Id = cityId,
            Name = "Test City",
            State = new State
            {
                Id = Guid.NewGuid(),
                Name = "Test State"
            }
        };
        
        var deliveryFee = new DeliveryFee
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            City = city,
            Fee = DeliveryFee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.DeliveryFees.AddAsync(deliveryFee);
        await db.SaveChangesAsync();
        
        // Act
        var getDeliveryFeesResponse = await Client.GetAsync("api/orders/delivery-fees");

        // Assert
        getDeliveryFeesResponse.EnsureSuccessStatusCode();

        var deliveryFees = await getDeliveryFeesResponse.DeserializeAsync<IEnumerable<DeliveryFeeResponse>>();

        deliveryFees.ShouldNotBeNull().ShouldNotBeEmpty();
    }
    
    [Fact(DisplayName = "Should get delivery fee")]
    public async Task ShouldGetDeliveryFee()
    {
        // Act
        var getDeliveryFeeResponse = await Client.GetAsync($"api/orders/delivery-fees/{cityId}");

        // Assert
        getDeliveryFeeResponse.EnsureSuccessStatusCode();

        var deliveryFee = await getDeliveryFeeResponse.DeserializeAsync<DeliveryFeeResponse>();

        deliveryFee.ShouldNotBeNull();
        deliveryFee.Fee.ShouldBe(DeliveryFee);
    }
}
