using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Products;
using Application.Products.Options;
using Application.Products.Options.CreateType;
using Application.Products.SearchAll;
using Application.Products.TopSelling;
using Application.Products.Update;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Domain.Categories;
using Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class ProductsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid categoryId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
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

        var product = await response.DeserializeAsync<ProductResponse>();

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
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test 2",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var newProduct = _productCommandFaker
            .WithCategoryId(category.Id)
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("api/products/for-sale");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<IEnumerable<SimpleProductForSaleResponse>>();

        products.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return a product by id")]
    public async Task ShouldReturnAProductById()
    {
        // Arrange
        var category = new Category
        {
            Id = categoryId,
            Name = "Category Test",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var newProduct = _productCommandFaker.WithCategoryId(categoryId).Generate();
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

    [Fact(DisplayName = "Should return product suggestions")]
    public async Task ShouldReturnProductSuggestions()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test 2",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var newProduct = _productCommandFaker.WithCategoryId(category.Id).Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        newProduct = _productCommandFaker.WithCategoryId(category.Id).Generate();
        createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Act
        var response =
            await Client.GetAsync($"api/products/suggestions?productId={product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var suggestions = await response.DeserializeAsync<IEnumerable<SimpleProductForSaleResponse>>();

        suggestions.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should update a product", Skip = "ExecuteDeleteAsync is not supporter for InMemoryDatabase")]
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
            newProduct.Images,
            newProduct.CategoryId,
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
        var newProduct = _productCommandFaker.Generate();
        var id = Guid.NewGuid();
        var updateProduct =
            new UpdateProductCommand(
                id,
                "test-update",
                "test-update",
                2000,
                20,
                newProduct.Images,
                Guid.NewGuid(),
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
                [],
                Guid.NewGuid(),
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

    [Fact(DisplayName = "Should return top selling products")]
    public async Task ShouldReturnTopSellingProducts()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test Top Selling",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create products directly in the database with different sales counts
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Description = "Description 1",
                Price = 100,
                Stock = 10,
                Enabled = true,
                SalesCount = 50,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Description = "Description 2",
                Price = 200,
                Stock = 20,
                Enabled = true,
                SalesCount = 100,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Product 3",
                Description = "Description 3",
                Price = 300,
                Stock = 30,
                Enabled = true,
                SalesCount = 75,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/top-selling");

        // Assert
        response.EnsureSuccessStatusCode();

        var body = await response.DeserializeAsync<IEnumerable<TopSellingProductResponse>>();

        var topSellingProducts = body?.ToList();
        topSellingProducts.ShouldNotBeNull();
        topSellingProducts.ShouldNotBeEmpty();

        // Verify products are ordered by sales count
        topSellingProducts[0].Name.ShouldBe("Product 2"); // Highest sales count (100)
        topSellingProducts[1].Name.ShouldBe("Product 3"); // Second-highest sales count (75)
        topSellingProducts[2].Name.ShouldBe("Product 1"); // Lowest sales count (50)
    }

    [Fact(DisplayName = "Should return a product by slug")]
    public async Task ShouldReturnAProductBySlug()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var newProduct = _productCommandFaker.WithCategoryId(category.Id).Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        // Act
        var response = await Client.GetAsync($"api/products/slug/{product.Slug}");

        // Assert
        response.EnsureSuccessStatusCode();

        var productResponse = await response.DeserializeAsync<ProductResponse>();

        productResponse.ShouldNotBeNull();
        productResponse.Id.ShouldBe(product.Id);
        productResponse.Slug.ShouldBe(product.Slug);
    }

    [Fact(DisplayName = "Should return not found when product slug not exists")]
    public async Task ShouldReturnNotFoundWhenProductSlugNotExists()
    {
        // Arrange
        const string nonExistentSlug = "non-existent-product-1234";

        // Act
        var response = await Client.GetAsync($"api/products/slug/{nonExistentSlug}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should return all featured products")]
    public async Task ShouldReturnAllFeaturedProducts()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test Featured",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var featuredProducts = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 1",
                Description = "Description 1",
                Price = 100,
                Stock = 10,
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Description = "Description 2",
                Price = 200,
                Stock = 20,
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await db.Products.AddRangeAsync(featuredProducts);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/featured");

        // Assert
        response.EnsureSuccessStatusCode();

        var body = await response.DeserializeAsync<List<SimpleProductForSaleResponse>>();

        body.ShouldNotBeNull();
        body.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return all popular products")]
    public async Task ShouldReturnAllPopularProducts()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test Popular",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create popular products directly in the database
        var popularProducts = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Popular Product 1",
                Description = "Description 1",
                Price = 100,
                Stock = 10,
                Enabled = true,
                Rating = 4.5m,
                SalesCount = 50,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Popular Product 2",
                Description = "Description 2",
                Price = 200,
                Stock = 20,
                Enabled = true,
                Rating = 4.8m,
                SalesCount = 100,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await db.Products.AddRangeAsync(popularProducts);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/popular");

        // Assert
        response.EnsureSuccessStatusCode();

        var body =
            await response.DeserializeAsync<List<SimpleProductForSaleResponse>>();

        body.ShouldNotBeNull();
        body.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return all products by category")]
    public async Task ShouldReturnAllProductsByCategory()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Category Test By Category",
            CreatedAt = DateTime.UtcNow
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Categories.AddAsync(category);
        await db.SaveChangesAsync();

        var newProduct = _productCommandFaker.WithCategoryId(category.Id).Generate();
        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync($"api/products/category/{category.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<List<SimpleProductForSaleResponse>>();

        products.ShouldNotBeEmpty();
        products.ShouldAllBe(p => p.CategoryId == category.Id);
    }

    [Fact(DisplayName = "Should create a product option type")]
    public async Task ShouldCreateProductOptionType()
    {
        // Arrange
        var command = new CreateProductOptionTypeCommand("Color", ["Red", "Blue", "Green"]);

        // Act
        var response = await Client.PostAsJsonAsync("api/products/option-types", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Should return all product option types")]
    public async Task ShouldReturnAllProductOptionTypes()
    {
        // Arrange
        var command = new CreateProductOptionTypeCommand("Size", ["Small", "Medium", "Large"]);
        await Client.PostAsJsonAsync("api/products/option-types", command);

        // Act
        var response = await Client.GetAsync("api/products/option-types");

        // Assert
        response.EnsureSuccessStatusCode();

        var optionTypes = await response.DeserializeAsync<List<ProductOptionTypeResponse>>();

        optionTypes.ShouldNotBeEmpty();
    }
}
