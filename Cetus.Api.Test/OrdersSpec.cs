using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Orders;
using Application.Orders.CalculateInsights;
using Application.Orders.Cancel;
using Application.Orders.ChangeStatus;
using Application.Orders.Create;
using Application.Orders.CreateSale;
using Application.Orders.DeliveryFees.Create;
using Application.Orders.DeliveryFees.Find;
using Application.Orders.Find;
using Application.Orders.SearchTimeline;
using Application.Orders.Summary;
using Bogus;
using Bogus.Extensions.Belgium;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Customers;
using Domain.Orders;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Shouldly;

namespace Cetus.Api.Test;

public class OrdersSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private const decimal DeliveryFee = 100m;
    private readonly Faker _faker = new();

    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");

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

    [Fact(DisplayName = "Should create a new order")]
    public async Task ShouldCreateANewOrder()
    {
        // Arrange 
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

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
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product, 50);

        // Act
        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should get an order")]
    public async Task ShouldGetAnOrder()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

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
        orderResponse.Status.ShouldBe(OrderStatus.PendingPayment);
    }

    [Fact(DisplayName = "Should get all orders")]
    public async Task ShouldGetAllOrders()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        // Act
        var getOrdersResponse = await Client.GetAsync("api/orders");

        // Assert
        getOrdersResponse.EnsureSuccessStatusCode();

        var orders = await getOrdersResponse.DeserializeAsync<PagedResult<OrderResponse>>();

        orders.ShouldNotBeNull();
        orders.Items.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should deliver an order")]
    public async Task ShouldDeliverAnOrder()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var createResponse = await Client.PostAsJsonAsync("api/orders", newOrder);

        createResponse.EnsureSuccessStatusCode();

        var createdOrder = await createResponse.DeserializeAsync<OrderResponse>();
        createdOrder.ShouldNotBeNull();

        // Act
        await OrderHelper.ChangeStatusThrough(
            Client,
            createdOrder.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        var order = await Client.GetAsync($"api/orders/{createdOrder.Id}");

        order.EnsureSuccessStatusCode();

        var orderResponse = await order.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(createdOrder.Id);
        orderResponse.Status.ShouldBe(OrderStatus.Delivered);
    }

    [Fact(DisplayName = "Should cancel an order")]
    public async Task ShouldCancelAnOrder()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var cancelOrderCommand = new CancelOrderCommand(orderId.Id, "Customer requested cancellation", "admin");
        var cancelOrderResponse = await Client.PostAsJsonAsync($"api/orders/{orderId.Id}/cancel", cancelOrderCommand);

        // Assert
        cancelOrderResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId.Id}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId.Id);
        orderResponse.Status.ShouldBe(OrderStatus.Canceled);
    }

    [Fact(DisplayName = "Should not cancel an already canceled order")]
    public async Task ShouldNotCancelAnAlreadyCanceledOrder()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        var cancelOrderCommand = new CancelOrderCommand(orderId.Id, "Customer requested cancellation", "admin");
        var cancelOrderResponse = await Client.PostAsJsonAsync($"api/orders/{orderId.Id}/cancel", cancelOrderCommand);

        cancelOrderResponse.EnsureSuccessStatusCode();

        // Act
        var secondCancelOrderCommand =
            new CancelOrderCommand(orderId.Id, "Customer requested cancellation again", "admin");
        var secondCancelOrderResponse =
            await Client.PostAsJsonAsync($"api/orders/{orderId.Id}/cancel", secondCancelOrderCommand);

        // Assert
        secondCancelOrderResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Should get orders insights")]
    public async Task ShouldGetOrdersInsights()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        await OrderHelper.ChangeStatusThrough(
            Client,
            order.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        // Act
        string month = DateTime.UtcNow.ToString("MMMM", CultureInfo.InvariantCulture);
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
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);
        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        await OrderHelper.ChangeStatusThrough(
            Client,
            order.Id,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        );

        // Act
        string month = DateTime.UtcNow.ToString("MMMM", CultureInfo.InvariantCulture);
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
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var deliveryFee = new DeliveryFee
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            Fee = DeliveryFee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

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

    [Fact(DisplayName = "Should change order status")]
    public async Task ShouldChangeOrderStatus()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var changeStatusCommand = new ChangeOrderStatusCommand(
            orderId.Id,
            OrderStatus.PaymentConfirmed,
            PaymentMethod.CashOnDelivery,
            PaymentStatus.Verified,
            "system",
            "Notes"
        );
        var changeStatusResponse =
            await Client.PutAsJsonAsync($"api/orders/{orderId.Id}/status", changeStatusCommand);

        // Assert
        changeStatusResponse.EnsureSuccessStatusCode();

        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId.Id}");

        getOrderResponse.EnsureSuccessStatusCode();

        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();

        orderResponse.ShouldNotBeNull();
        orderResponse.Id.ShouldBe(orderId.Id);
        orderResponse.Status.ShouldBe(OrderStatus.PaymentConfirmed);
    }

    [Fact(DisplayName = "Should not change order status with invalid transition")]
    public async Task ShouldNotChangeOrderStatusWithInvalidTransition()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var orderId = await response.DeserializeAsync<OrderResponse>();
        orderId.ShouldNotBeNull();

        // Act
        var changeStatusCommand = new ChangeOrderStatusCommand(orderId.Id, OrderStatus.Shipped);
        var changeStatusResponse =
            await Client.PutAsJsonAsync($"api/orders/{orderId.Id}/status", changeStatusCommand);

        // Assert
        changeStatusResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should return order timeline")]
    public async Task ShouldReturnOrderTimeline()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        await CityHelper.CreateIfNotExists(cityId, db);

        var product = await ProductHelper.CreateProductWithVariant(Client);
        var newOrder = GenerateCreateOrderCommand(product);

        var response = await Client.PostAsJsonAsync("api/orders", newOrder);

        response.EnsureSuccessStatusCode();

        var order = await response.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        await OrderHelper.ChangeStatus(Client, order.Id, OrderStatus.PaymentConfirmed);

        // Act
        var getOrderTimelineResponse = await Client.GetAsync($"api/orders/{order.Id}/timeline");

        // Assert
        getOrderTimelineResponse.EnsureSuccessStatusCode();

        var orderTimeline = await getOrderTimelineResponse.DeserializeAsync<IEnumerable<OrderTimelineResponse>>();

        orderTimeline.ShouldNotBeNull().ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should create a new sale")]
    public async Task ShouldCreateANewSale()
    {
        // Arrange 
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var newCustomer = new CreateSaleCustomer(
            _faker.Phone.PhoneNumber("##########"),
            _faker.Name.FullName(),
            _faker.Internet.Email(),
            DocumentType.CC,
            _faker.Person.NationalNumber()
        );

        var newSaleItems = new List<CreateSaleItem>
        {
            new(product.VariantId, 1)
        };

        var newSale = new CreateSaleCommand(
            newSaleItems,
            newCustomer,
            OrderChannel.InStore,
            PaymentMethod.Nequi
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/sales", newSale);

        // Assert
        response.EnsureSuccessStatusCode();

        var sale = await response.DeserializeAsync<SimpleOrderResponse>();

        sale.ShouldNotBeNull();
    }
}
