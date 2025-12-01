using System.Net.Http.Json;
using Application.Categories.Create;
using Application.Categories.SearchAll;
using Application.Products;
using Application.Products.Create;
using Application.Products.Variants;
using Application.Products.Variants.Create;
using Bogus;
using Cetus.Api.Test.Shared.Fakers;
using Shouldly;

namespace Cetus.Api.Test.Shared.Helpers;

public sealed record CreateProductWithVariantResponse(
    Guid Id,
    long VariantId,
    string Name,
    decimal Price,
    string ImageUrl);

public static class ProductHelper
{
    private static readonly CreateProductCommandFaker ProductCommandFaker = new();
    private static readonly Faker Faker = new();

    private static Guid? _categoryId;

    public static async Task<CreateProductWithVariantResponse> CreateProductWithVariant(HttpClient client)
    {
        await SetCategoryIdIfDontExists(client);

        var newProduct = ProductCommandFaker
            .WithCategoryId(_categoryId ?? Guid.NewGuid())
            .Generate();

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
            [new CreateProductImage(Faker.Image.PicsumUrl(), Faker.Commerce.ProductName(), 0)]
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

    public static async Task<Guid> GetOrCreateCategoryId(HttpClient client)
    {
        await SetCategoryIdIfDontExists(client);
        return _categoryId ?? Guid.NewGuid();
    }

    private static async Task SetCategoryIdIfDontExists(HttpClient client)
    {
        if (_categoryId.HasValue)
        {
            return;
        }

        var newCategory = new CreateCategoryCommand(Faker.Commerce.Categories(1)[0]);
        var response = await client.PostAsJsonAsync("api/categories", newCategory);

        response.EnsureSuccessStatusCode();

        var category = await response.DeserializeAsync<CategoryResponse>();

        category.ShouldNotBeNull();

        _categoryId = category.Id;
    }
}
