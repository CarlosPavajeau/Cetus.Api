using System.Net.Http.Json;
using Cetus.Api.Test.Shared;
using Cetus.Application.CreateProduct;
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
    }
}
