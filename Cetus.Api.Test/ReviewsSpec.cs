using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Orders.Create;
using Application.Orders.Find;
using Application.Products.SearchAll;
using Application.Reviews.ProductReviews.Create;
using Application.Reviews.ProductReviews.Reject;
using Application.Reviews.ProductReviews.SearchAll;
using Application.Reviews.ReviewRequests.Find;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
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
        // Arrange 
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
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
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
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

    [Fact(DisplayName = "Should return all reviews for a product")]
    public async Task ShouldReturnAllReviewsForProduct()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        // Arrange - Create multiple orders and reviews
        var db = Services.GetRequiredService<IApplicationDbContext>();
        const int reviewCount = 3;
        var createdReviews = new List<ProductReview>();

        for (var i = 0; i < reviewCount; i++)
        {
            // Create order
            var newCustomer = _orderCustomerFaker.Generate();
            var newOrderItems = new List<CreateOrderItem>
            {
                new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
            };

            var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
                newCustomer);
            var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
            createOrderResponse.EnsureSuccessStatusCode();
            var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
            order.ShouldNotBeNull();

            // Deliver order
            var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
            deliverOrderResponse.EnsureSuccessStatusCode();

            // Get review request
            var reviewRequest = await db.ReviewRequests
                .Include(r => r.Customer)
                .Include(r => r.OrderItem)
                .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
            reviewRequest.ShouldNotBeNull();

            // Create review
            var createReviewCommand = new CreateProductReviewCommand(
                reviewRequest.Id,
                _faker.Lorem.Paragraph(),
                _faker.Random.Byte(1, 5));

            var createReviewResponse = await Client.PostAsJsonAsync("api/reviews/products", createReviewCommand);
            createReviewResponse.EnsureSuccessStatusCode();

            // Get created review
            var createdReview = await db.ProductReviews
                .FirstOrDefaultAsync(r => r.ReviewRequestId == reviewRequest.Id);

            createdReview.ShouldNotBeNull();

            createdReview.Status = ProductReviewStatus.Approved;
            createdReviews.Add(createdReview);
        }

        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/reviews/products/{product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var reviews = await response.DeserializeAsync<List<ProductReviewResponse>>();
        reviews.ShouldNotBeNull();
        reviews.Count.ShouldBe(reviewCount);

        // Verify each review's data
        foreach (var review in reviews)
        {
            var originalReview = createdReviews.First(r => r.Id == review.Id);
            review.Rating.ShouldBe(originalReview.Rating);
            review.Comment.ShouldBe(originalReview.Comment);
            review.Customer.ShouldNotBeNull();
            review.CreatedAt.ShouldBe(originalReview.CreatedAt);
        }
    }

    [Fact(DisplayName = "Should return empty list for product with no reviews")]
    public async Task ShouldReturnEmptyListForProductWithNoReviews()
    {
        // Arrange - Create a product
        var newProduct = _productCommandFaker.Generate();
        var createProductResponse = await Client.PostAsJsonAsync("api/products", newProduct);
        createProductResponse.EnsureSuccessStatusCode();
        var product = await createProductResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Act
        var response = await Client.GetAsync($"api/reviews/products/{product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var reviews = await response.DeserializeAsync<List<ProductReviewResponse>>();
        reviews.ShouldNotBeNull();
        reviews.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Should return pending approval reviews")]
    public async Task ShouldReturnPendingApprovalReviews()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        // Arrange - Create multiple orders and reviews
        var db = Services.GetRequiredService<IApplicationDbContext>();
        const int reviewCount = 3;

        for (var i = 0; i < reviewCount; i++)
        {
            // Create order
            var newCustomer = _orderCustomerFaker.Generate();
            var newOrderItems = new List<CreateOrderItem>
            {
                new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
            };

            var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
                newCustomer);
            var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
            createOrderResponse.EnsureSuccessStatusCode();
            var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
            order.ShouldNotBeNull();

            // Deliver order
            var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
            deliverOrderResponse.EnsureSuccessStatusCode();

            // Get review request
            var reviewRequest = await db.ReviewRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
            reviewRequest.ShouldNotBeNull();

            // Create review
            var createReviewCommand = new CreateProductReviewCommand(
                reviewRequest.Id,
                _faker.Lorem.Paragraph(),
                _faker.Random.Byte(1, 5));

            var createReviewResponse = await Client.PostAsJsonAsync("api/reviews/products", createReviewCommand);
            createReviewResponse.EnsureSuccessStatusCode();

            // Get created review
            var createdReview = await db.ProductReviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReviewRequestId == reviewRequest.Id);
            createdReview.ShouldNotBeNull();
        }

        // Act
        var response = await Client.GetAsync("api/reviews/products/pending");

        // Assert
        response.EnsureSuccessStatusCode();
        var reviews = await response.DeserializeAsync<List<ProductReviewResponse>>();
        reviews.ShouldNotBeNull();
        reviews.Count.ShouldBeGreaterThanOrEqualTo(reviewCount);
    }

    [Fact(DisplayName = "Should approve a review")]
    public async Task ShouldApproveReview()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
        };
        
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
            newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        // Deliver order
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
        deliverOrderResponse.EnsureSuccessStatusCode();

        // Get review request
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var reviewRequest = await db.ReviewRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
        reviewRequest.ShouldNotBeNull();

        // Create review
        var createReviewCommand = new CreateProductReviewCommand(
            reviewRequest.Id,
            _faker.Lorem.Paragraph(),
            _faker.Random.Byte(1, 5));

        var createReviewResponse = await Client.PostAsJsonAsync("api/reviews/products", createReviewCommand);
        createReviewResponse.EnsureSuccessStatusCode();

        // Get created review
        var createdReview = await db.ProductReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReviewRequestId == reviewRequest.Id);
        createdReview.ShouldNotBeNull();

        // Act
        var response = await Client.PostAsync($"api/reviews/products/{createdReview.Id}/approve", null);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify review was approved
        var updatedReview = await db.ProductReviews.FindAsync(createdReview.Id);
        updatedReview.ShouldNotBeNull();
        updatedReview.Status.ShouldBe(ProductReviewStatus.Approved);
    }

    [Fact(DisplayName = "Should reject a review")]
    public async Task ShouldRejectReview()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var newCustomer = _orderCustomerFaker.Generate();
        var newOrderItems = new List<CreateOrderItem>
        {
            new(product.Name, product.ImageUrl, 1, product.Price, product.VariantId)
        };
        
        var newOrder = new CreateOrderCommand(_faker.Address.FullAddress(), cityId, product.Price, newOrderItems,
            newCustomer);
        var createOrderResponse = await Client.PostAsJsonAsync("api/orders", newOrder);
        createOrderResponse.EnsureSuccessStatusCode();
        var order = await createOrderResponse.DeserializeAsync<OrderResponse>();
        order.ShouldNotBeNull();

        // Deliver order
        var deliverOrderResponse = await Client.PostAsync($"api/orders/{order.Id}/deliver", null);
        deliverOrderResponse.EnsureSuccessStatusCode();

        // Get review request
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var reviewRequest = await db.ReviewRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.CustomerId == newCustomer.Id);
        reviewRequest.ShouldNotBeNull();

        // Create review
        var createReviewCommand = new CreateProductReviewCommand(
            reviewRequest.Id,
            _faker.Lorem.Paragraph(),
            _faker.Random.Byte(1, 5));

        var createReviewResponse = await Client.PostAsJsonAsync("api/reviews/products", createReviewCommand);
        createReviewResponse.EnsureSuccessStatusCode();

        // Get created review
        var createdReview = await db.ProductReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReviewRequestId == reviewRequest.Id);
        createdReview.ShouldNotBeNull();

        // Act
        var request = new RejectProductReviewCommand(createdReview.Id, "Inappropriate content");
        var response = await Client.PostAsJsonAsync($"api/reviews/products/{createdReview.Id}/reject", request);

        // Assert
        response.EnsureSuccessStatusCode();

        // Verify review was rejected
        var updatedReview = await db.ProductReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(pr => pr.ReviewRequestId == reviewRequest.Id);

        updatedReview.ShouldNotBeNull();
        updatedReview.Status.ShouldBe(ProductReviewStatus.Rejected);
        updatedReview.ModeratorNotes.ShouldBe(request.ModeratorNotes);
    }
}
