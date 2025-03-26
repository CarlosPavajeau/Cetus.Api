using System.Net;
using System.Net.Http.Json;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Products.Application.SearchAll;
using Cetus.Products.Application.Update;
using Cetus.Products.Domain;
using Shouldly;

namespace Cetus.Api.Test;

public class ProductsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    
    [Fact(DisplayName = "Should create a new product")]
    public async Task ShouldCreateANewProduct()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();

        // Act
        var response = await Client.PostAsJsonAsync("api/products", newProduct);

        // Assert
        response.EnsureSuccessStatusCode();

        var product = await response.DeserializeAsync<Product>();

        product.ShouldNotBeNull();
        product.Enabled.ShouldBeTrue();
    }

    [Fact(DisplayName = "Should return all products")]
    public async Task ShouldReturnAllProducts()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();

        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("api/products");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<IEnumerable<ProductResponse>>();

        products.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return all products for sale")]
    public async Task ShouldReturnAllProductsForSale()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("api/products/for-sale");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<IEnumerable<ProductResponse>>();

        products.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return a product by id")]
    public async Task ShouldReturnAProductById()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Act
        var response = await Client.GetAsync($"api/products/{product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var productResponse = await response.DeserializeAsync<ProductResponse>();

        productResponse.ShouldNotBeNull();
        productResponse.Id.ShouldBe(product.Id);
    }

    [Fact(DisplayName = "Should return not found when product not exists")]
    public async Task ShouldReturnNotFoundWhenProductNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"api/products/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should update a product")]
    public async Task ShouldUpdateAProduct()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var updateProduct = new UpdateProductCommand(
            product.Id,
            newProduct.Name,
            newProduct.Description,
            2000,
            20,
            newProduct.ImageUrl,
            true
        );

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/{product.Id}", updateProduct);

        // Assert
        response.EnsureSuccessStatusCode();

        var updated = await response.DeserializeAsync<ProductResponse>();

        updated.ShouldNotBeNull();
        updated.Price.ShouldBe(2000);
        updated.Stock.ShouldBe(20);
    }

    [Fact(DisplayName = "Should return not found when updating a product that not exists")]
    public async Task ShouldReturnNotFoundWhenUpdatingAProductThatNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateProduct =
            new UpdateProductCommand(
                id,
                "test-update",
                "test-update",
                2000,
                20,
                "image-test",
                true
            );

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/{id}", updateProduct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should return bad request when updating a product with different id")]
    public async Task ShouldReturnBadRequestWhenUpdatingAProductWithDifferentId()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var updateProduct =
            new UpdateProductCommand(
                Guid.NewGuid(),
                "test-update",
                "test-update",
                2000,
                20,
                "image-test",
                true
            );

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/{product.Id}", updateProduct);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should delete a product")]
    public async Task ShouldDeleteAProduct()
    {
        // Arrange
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Act
        var response = await Client.DeleteAsync($"api/products/{product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Should return not found when deleting a product that not exists")]
    public async Task ShouldReturnNotFoundWhenDeletingAProductThatNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"api/products/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
