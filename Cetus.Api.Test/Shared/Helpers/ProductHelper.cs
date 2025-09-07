using System.Net.Http.Json;
using Application.Products.Create;
using Application.Products.SearchAll;
using Application.Products.Variants;
using Application.Products.Variants.Create;
using Bogus;
using Cetus.Api.Test.Shared.Fakers;
using Shouldly;

namespace Cetus.Api.Test.Shared.Helpers;

public sealed record CreateProductWithVariantResponse(Guid Id, long VariantId, string Name, decimal Price, string ImageUrl);

public static class ProductHelper
{
    private static readonly CreateProductCommandFaker _productCommandFaker = new();
    private static readonly Faker _faker = new();

    public static async Task<CreateProductWithVariantResponse> CreateProductWithVariant(HttpClient client)
    {
        var newProduct = _productCommandFaker.Generate();
        var createResponse = await client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();

        product.ShouldNotBeNull();

        var command = new CreateProductVariantCommand(
            product.Id,
            Guid.NewGuid().ToString(),
            100.00m,
            10,
            [],
            [new CreateProductImage(_faker.Image.PicsumUrl(), _faker.Commerce.ProductName(), 0)]
        );

        // Act
        var response = await client.PostAsJsonAsync($"api/products/{product.Id}/variants", command);

        response.EnsureSuccessStatusCode();

        var productVariant = await response.DeserializeAsync<ProductVariantResponse>();

        productVariant.ShouldNotBeNull();

        return new CreateProductWithVariantResponse(
            product.Id,
            productVariant.Id,
            product.Name,
            productVariant.Price,
            command.Images[0].ImageUrl
        );
    }
}
