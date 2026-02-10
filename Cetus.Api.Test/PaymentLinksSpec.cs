using System.Net;
using System.Net.Http.Json;
using Application.Orders.ChangeStatus;
using Application.PaymentLinks;
using Application.PaymentLinks.Create;
using Application.PaymentLinks.FindState;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Orders;
using Shouldly;

namespace Cetus.Api.Test;

public class PaymentLinksSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should generate a payment link for an order")]
    public async Task ShouldGenerateAPaymentLinkForAnOrder()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Act
        var response =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);

        // Assert
        response.EnsureSuccessStatusCode();

        var paymentLinkResponse = await response.DeserializeAsync<PaymentLinkResponse>();

        paymentLinkResponse.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should not generate a payment link for a non-existent order")]
    public async Task ShouldNotGenerateAPaymentLinkForANonExistentOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{orderId}/payment-link", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should not generate a payment link for a canceled order")]
    public async Task ShouldNotGenerateAPaymentLinkForACanceledOrder()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Cancel the order
        var changeOrderStatus = new ChangeOrderStatusCommand(order.Id, OrderStatus.Canceled);
        var cancelResponse = await Client.PutAsJsonAsync($"api/orders/{order.Id}/status", changeOrderStatus);
        cancelResponse.EnsureSuccessStatusCode();

        // Act
        var response =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Should not generate a payment link for an already paid order")]
    public async Task ShouldNotGenerateAPaymentLinkForAnAlreadyPaidOrder()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Mark the order as paid
        var changeOrderStatus = new ChangeOrderStatusCommand(
            order.Id,
            OrderStatus.PaymentConfirmed,
            PaymentMethod.CashOnDelivery,
            PaymentStatus.Verified
        );
        var paidResponse = await Client.PutAsJsonAsync($"api/orders/{order.Id}/status", changeOrderStatus);
        paidResponse.EnsureSuccessStatusCode();

        // Act
        var response =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Should not generate a new payment link if an active one already exists")]
    public async Task ShouldNotGenerateANewPaymentLinkIfAnActiveOneAlreadyExists()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Generate the first payment link
        var firstResponse =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);
        firstResponse.EnsureSuccessStatusCode();

        // Act
        var secondResponse =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Should find a payment link by its token")]
    public async Task ShouldFindAPaymentLinkByItsToken()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Generate a payment link
        var response =
            await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);
        response.EnsureSuccessStatusCode();

        var paymentLinkResponse = await response.DeserializeAsync<PaymentLinkResponse>();
        paymentLinkResponse.ShouldNotBeNull();

        // Act
        var findResponse = await Client.GetAsync($"api/payment-links/{paymentLinkResponse.Token}");

        // Assert
        findResponse.EnsureSuccessStatusCode();

        var foundPaymentLink = await findResponse.DeserializeAsync<PaymentLinkResponse>();

        foundPaymentLink.ShouldNotBeNull();
        foundPaymentLink.Token.ShouldBe(paymentLinkResponse.Token);
    }

    [Fact(DisplayName = "Should return not found when trying to find a non-existent payment link")]
    public async Task ShouldReturnNotFoundWhenTryingToFindANonExistentPaymentLink()
    {
        // Arrange
        string token = Guid.NewGuid().ToString();

        // Act
        var response = await Client.GetAsync($"api/payment-links/{token}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should find the state of a payment link for an order")]
    public async Task ShouldFindTheStateOfAPaymentLinkForAnOrder()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var order = await OrderHelper.CreateOrder(Client, product.VariantId);

        // Generate a payment link
        var response = await Client.PostAsJsonAsync<CreatePaymentLinkCommand?>($"api/orders/{order.Id}/payment-link", null);
        response.EnsureSuccessStatusCode();

        // Act
        var stateResponse = await Client.GetAsync($"api/orders/{order.Id}/payment-link");

        // Assert
        stateResponse.EnsureSuccessStatusCode();

        var paymentLinkState = await stateResponse.DeserializeAsync<PaymentLinkStateResponse>();

        paymentLinkState.ShouldNotBeNull();
        paymentLinkState.ActiveLink.ShouldNotBeNull();
    }
}
