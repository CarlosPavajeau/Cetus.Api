using System.Net;
using System.Net.Http.Json;
using Cetus.Api.Test.Shared;
using Cetus.Application.CreateProduct;
using Cetus.Application.SearchAllProducts;
using Cetus.Domain;
using Shouldly;

namespace Cetus.Api.Test;

public class Products(ApplicationTestCase factory)
    : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should create a new product")]
    public async Task ShouldCreateANewProduct()
    {
        // Arrange
        var newProduct =
            new CreateProductCommand("Test", null, 1500, 10, Guid.NewGuid());

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
        var newProduct =
            new CreateProductCommand("test-create", null, 1500, 10, Guid.NewGuid());
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("api/products");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<IEnumerable<ProductResponse>>();

        products.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return a product by id")]
    public async Task ShouldReturnAProductById()
    {
        // Arrange
        var newProduct =
            new CreateProductCommand("test-find", null, 1500, 10, Guid.NewGuid());
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
}
