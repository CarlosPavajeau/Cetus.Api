using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Bogus;
using Cetus.Api.Requests;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Orders.Application.Create;
using Cetus.Orders.Application.Find;
using Cetus.Orders.Domain;
using Cetus.Products.Application.SearchAll;
using Shouldly;

namespace Cetus.Api.Test;

public class WompiControllerSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Faker _faker = new();
    
    [Fact(DisplayName = "Should process approved Wompi transaction")]
    public async Task ShouldProcessApprovedWompiTransaction()
    {
        // Arrange - Create a product
        var newProduct = _productCommandFaker.WithStock(100).Generate();
        var createProductResponse = await Client.PostAsJsonAsync("api/products", newProduct);
        createProductResponse.EnsureSuccessStatusCode();
        var product = await createProductResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Arrange - Create an order
        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var orderId = await createOrderResponse.DeserializeAsync<Guid>();
        orderId.ShouldNotBe(Guid.Empty);

        // Arrange - Create a Wompi request
        var wompiRequest = CreateWompiRequest(orderId, "APPROVED", product.Price * 100);

        // Act
        var response = await Client.PostAsJsonAsync("api/wompi", wompiRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify order status was updated
        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId}");
        getOrderResponse.EnsureSuccessStatusCode();
        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();
        orderResponse.ShouldNotBeNull();
        orderResponse.Status.ShouldBe(OrderStatus.Paid);
    }
    
    [Fact(DisplayName = "Should process non-approved Wompi transaction")]
    public async Task ShouldProcessNonApprovedWompiTransaction()
    {
        // Arrange - Create a product
        var newProduct = _productCommandFaker.WithStock(100).Generate();
        var createProductResponse = await Client.PostAsJsonAsync("api/products", newProduct);
        createProductResponse.EnsureSuccessStatusCode();
        var product = await createProductResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Arrange - Create an order
        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var orderId = await createOrderResponse.DeserializeAsync<Guid>();
        orderId.ShouldNotBe(Guid.Empty);

        // Arrange - Create a Wompi request with DECLINED status
        var wompiRequest = CreateWompiRequest(orderId, "DECLINED", product.Price * 100);

        // Act
        var response = await Client.PostAsJsonAsync("api/wompi", wompiRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify order status remains Pending
        var getOrderResponse = await Client.GetAsync($"api/orders/{orderId}");
        getOrderResponse.EnsureSuccessStatusCode();
        var orderResponse = await getOrderResponse.DeserializeAsync<OrderResponse>();
        orderResponse.ShouldNotBeNull();
        orderResponse.Status.ShouldBe(OrderStatus.Pending);
    }
    
    [Fact(DisplayName = "Should reject Wompi request with invalid checksum")]
    public async Task ShouldRejectWompiRequestWithInvalidChecksum()
    {
        // Arrange - Create a product and order
        var newProduct = _productCommandFaker.WithStock(100).Generate();
        var createProductResponse = await Client.PostAsJsonAsync("api/products", newProduct);
        createProductResponse.EnsureSuccessStatusCode();
        var product = await createProductResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(newProduct.Name, newProduct.ImageUrl, 1, product.Price, product.Id)
        };
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems, newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var orderId = await createOrderResponse.DeserializeAsync<Guid>();

        // Arrange - Create a Wompi request with invalid checksum
        var wompiRequest = new WompiRequest(
            "transaction.updated",
            new WompiData(new WompiTransaction(
                "123456",
                orderId.ToString(),
                "APPROVED", 
                product.Price * 100
            )),
            "test",
            new WompiSignature(
                new[] { "transaction.id", "transaction.status", "transaction.amount_in_cents", "transaction.reference" },
                "invalid_checksum"
            ),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/wompi", wompiRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact(DisplayName = "Should reject Wompi request with invalid order ID")]
    public async Task ShouldRejectWompiRequestWithInvalidOrderId()
    {
        // Arrange - Create a Wompi request with invalid order ID
        var wompiRequest = CreateWompiRequest(Guid.NewGuid(), "APPROVED", 1000);

        // Act
        var response = await Client.PostAsJsonAsync("api/wompi", wompiRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact(DisplayName = "Should reject Wompi request with unparseable order ID")]
    public async Task ShouldRejectWompiRequestWithUnparseableOrderId()
    {
        // Arrange - Create a Wompi request with unparseable order ID
        var wompiRequest = new WompiRequest(
            "transaction.updated",
            new WompiData(new WompiTransaction(
                "123456",
                "not-a-guid",
                "APPROVED", 
                1000
            )),
            "test",
            new WompiSignature(
                new[] { "transaction.id", "transaction.status", "transaction.amount_in_cents", "transaction.reference" },
                ComputeValidChecksum("123456", "not-a-guid", "APPROVED", "1000", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            ),
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/wompi", wompiRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private WompiRequest CreateWompiRequest(Guid orderId, string status, decimal amountInCents)
    {
        var transactionId = _faker.Random.AlphaNumeric(10);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return new WompiRequest(
            "transaction.updated",
            new WompiData(new WompiTransaction(
                transactionId,
                orderId.ToString(),
                status, 
                amountInCents
            )),
            "test",
            new WompiSignature(
                ["transaction.id", "transaction.status", "transaction.amount_in_cents", "transaction.reference"],
                ComputeValidChecksum(transactionId, orderId.ToString(), status, amountInCents.ToString(CultureInfo.InvariantCulture), timestamp)
            ),
            timestamp
        );
    }
    
    private static string ComputeValidChecksum(string transactionId, string reference, string status, string amountInCents, long timestamp)
    {
        // This should match the algorithm in WompiController.ComputeChecksum
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(transactionId);
        stringBuilder.Append(status);
        stringBuilder.Append(amountInCents);
        stringBuilder.Append(reference);
        stringBuilder.Append(timestamp);
        
        // Use a hardcoded test event secret - this should match what's configured in test environment
        stringBuilder.Append("test_event_secret");
        
        var bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
        var hash = SHA256.HashData(bytes);
        
        var checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return checksum;
    }
} 
