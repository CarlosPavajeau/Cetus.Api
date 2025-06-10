using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Orders.Create;
using Application.Orders.Find;
using Application.Products.SearchAll;
using Application.Reviews.ProductReviews.Create;
using Application.Reviews.ReviewRequests.Find;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class ReviewsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid cityId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly CreateOrderCustomerFaker _orderCustomerFaker = new();
    private readonly Faker _faker = new();

    [Fact(DisplayName = "Should find a review request by token")]
    public async Task ShouldFindReviewRequestByToken()
    {
        // Arrange - Create a product
        var newProduct = _productCommandFaker.Generate();
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
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
            newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        // Arrange - Deliver the order to generate review request
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
        deliverOrderResponse.EnsureSuccessStatusCode();

        // Get the review request token from the database
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var reviewRequest = await db.ReviewRequests
            .Include(r => r.Customer)
            .Include(r => r.OrderItem)
            .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
        reviewRequest.ShouldNotBeNull();

        // Act
        var response = await Client.GetAsync($"api/reviews/requests/{reviewRequest.Token}");

        // Assert
        response.EnsureSuccessStatusCode();

        var reviewRequestResponse = await response.DeserializeAsync<ReviewRequestResponse>();
        reviewRequestResponse.ShouldNotBeNull();
        reviewRequestResponse.Id.ShouldBe(reviewRequest.Id);
        reviewRequestResponse.Status.ShouldBe(ReviewRequestStatus.Pending);
        reviewRequestResponse.Customer.ShouldBe(newCustomer.Name);
        reviewRequestResponse.Product.Name.ShouldBe(newProduct.Name);
        reviewRequestResponse.Product.ImageUrl.ShouldBe(newProduct.ImageUrl);
    }

    [Fact(DisplayName = "Should return not found for non-existing review request")]
    public async Task ShouldReturnNotFoundForNonExistingReviewRequest()
    {
        // Arrange
        var nonExistingToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "")[..22];

        // Act
        var response = await Client.GetAsync($"reviews/requests/{nonExistingToken}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should create a product review")]
    public async Task ShouldCreateProductReview()
    {
        // Arrange - Create a product
        var newProduct = _productCommandFaker.Generate();
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
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
            newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        // Arrange - Deliver the order to generate review request
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
        deliverOrderResponse.EnsureSuccessStatusCode();

        // Get the review request token from the database
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var reviewRequest = await db.ReviewRequests
            .Include(r => r.Customer)
            .Include(r => r.OrderItem)
            .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
        reviewRequest.ShouldNotBeNull();

        // Arrange - Create review command
        var createReviewCommand = new CreateProductReviewCommand(
            reviewRequest.Id,
            _faker.Lorem.Paragraph(),
            _faker.Random.Byte(1, 5));

        // Act
        var response = await Client.PostAsJsonAsync("api/reviews/products", createReviewCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify the review was created in the database
        var createdReview = await db.ProductReviews
            .FirstOrDefaultAsync(r => r.ReviewRequestId == reviewRequest.Id);
        createdReview.ShouldNotBeNull();
        createdReview.Rating.ShouldBe(createReviewCommand.Rating);
        createdReview.Comment.ShouldBe(createReviewCommand.Comment);
    }
} 
