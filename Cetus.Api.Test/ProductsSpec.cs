using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Products;
using Application.Products.Create;
using Application.Products.Inventory.Adjust;
using Application.Products.Inventory.Transactions;
using Application.Products.Options;
using Application.Products.Options.Create;
using Application.Products.Options.CreateType;
using Application.Products.TopSelling;
using Application.Products.Update;
using Application.Products.Variants;
using Application.Products.Variants.Create;
using Application.Products.Variants.Images.Add;
using Application.Products.Variants.Images.Order;
using Application.Products.Variants.Update;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using Shouldly;

namespace Cetus.Api.Test;

public class ProductsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly Faker _faker = new();

    [Fact(DisplayName = "Should create a new product")]
    public async Task ShouldCreateANewProduct()
    {
        // Arrange
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                ]
            }
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Products.AddRangeAsync(products);

        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/for-sale");

        // Assert
        response.EnsureSuccessStatusCode();

        var productsResponse = await response.DeserializeAsync<PagedResult<SimpleProductForSaleResponse>>();

        productsResponse.ShouldNotBeNull();
        productsResponse.Items.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should return a product by id")]
    public async Task ShouldReturnAProductById()
    {
        // Arrange
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                ]
            }
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();

        var product = products[0];

        // Act
        var response =
            await Client.GetAsync($"api/products/suggestions?productId={product.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var suggestions = await response.DeserializeAsync<IEnumerable<SimpleProductForSaleResponse>>();

        suggestions.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should update a product")]
    public async Task ShouldUpdateAProduct()
    {
        // Arrange
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var updateProduct = new UpdateProductCommand(
            product.Id,
            newProduct.Name,
            newProduct.Description,
            newProduct.CategoryId,
            true
        );

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/{product.Id}", updateProduct);

        // Assert
        response.EnsureSuccessStatusCode();

        var updated = await response.DeserializeAsync<ProductResponse>();

        updated.ShouldNotBeNull();
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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

        var createResponse = await Client.PostAsJsonAsync("api/products", newProduct);

        createResponse.EnsureSuccessStatusCode();

        var product = await createResponse.DeserializeAsync<ProductResponse>();
        product.ShouldNotBeNull();

        var updateProduct =
            new UpdateProductCommand(
                Guid.NewGuid(),
                "test-update",
                "test-update",
                categoryId,
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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create products directly in the database with different sales counts
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                ]
            }
        };

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/top-selling");

        // Assert
        response.EnsureSuccessStatusCode();

        var topSellingProducts = await response.DeserializeAsync<List<TopSellingProductResponse>>();

        topSellingProducts.ShouldNotBeNull();
        topSellingProducts.ShouldNotBeEmpty();

        // Verify products are ordered by sales count
        topSellingProducts[0].SalesCount.ShouldBeGreaterThanOrEqualTo(topSellingProducts[1].SalesCount);
    }

    [Fact(DisplayName = "Should return a product by slug")]
    public async Task ShouldReturnAProductBySlug()
    {
        // Arrange
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var newProduct = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var featuredProducts = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                ]
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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create popular products directly in the database
        var popularProducts = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                ]
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Commerce.ProductDescription(),
                Slug = _faker.Lorem.Slug(10),
                Enabled = true,
                CategoryId = categoryId,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants =
                [
                    new ProductVariant
                    {
                        Sku = _faker.Commerce.Ean13(),
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                ]
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
        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);
        var tenant = Services.GetRequiredService<ITenantContext>();

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Commerce.ProductDescription(),
            Slug = _faker.Lorem.Slug(10),
            Enabled = true,
            CategoryId = categoryId,
            StoreId = tenant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Variants =
            [
                new ProductVariant
                {
                    Sku = _faker.Commerce.Ean13(),
                    Price = 100,
                    Stock = 10,
                    Enabled = true,
                    Featured = true
                }
            ]
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();
        await db.Products.AddAsync(newProduct);

        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"api/products/category/{categoryId}");

        // Assert
        response.EnsureSuccessStatusCode();

        var products = await response.DeserializeAsync<List<SimpleProductForSaleResponse>>();

        products.ShouldNotBeEmpty();
        products.ShouldAllBe(p => p.CategoryId == categoryId);
    }

    [Fact(DisplayName = "Should create a product option type")]
    public async Task ShouldCreateProductOptionType()
    {
        // Arrange
        var command = new CreateProductOptionTypeCommand(
            $"{_faker.Commerce.ProductMaterial()}-{Guid.NewGuid():N}",
            [_faker.Lorem.Sentence(5), _faker.Lorem.Sentence(5), _faker.Lorem.Sentence(5)]
        );

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
        var command = new CreateProductOptionTypeCommand(
            $"{_faker.Commerce.ProductMaterial()}-{Guid.NewGuid():N}",
            [_faker.Lorem.Sentence(5), _faker.Lorem.Sentence(5), _faker.Lorem.Sentence(5)]
        );

        await Client.PostAsJsonAsync("api/products/option-types", command);

        // Act
        var response = await Client.GetAsync("api/products/option-types");

        // Assert
        response.EnsureSuccessStatusCode();

        var optionTypes = await response.DeserializeAsync<List<ProductOptionTypeResponse>>();

        optionTypes.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should create a product option")]
    public async Task ShouldCreateProductOption()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();
        var optionType = new ProductOptionType
        {
            Name = $"{_faker.Commerce.ProductMaterial()}-{Guid.NewGuid():N}",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue
                {
                    Value = _faker.Lorem.Sentence(5)
                }
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var product = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

        var createProductResponse = await Client.PostAsJsonAsync("api/products", product);
        createProductResponse.EnsureSuccessStatusCode();

        var createdProduct = await createProductResponse.DeserializeAsync<ProductResponse>();
        createdProduct.ShouldNotBeNull();

        var command = new CreateProductOptionCommand(createdProduct.Id, optionType.Id);

        // Act
        var response = await Client.PostAsJsonAsync($"api/products/{createdProduct.Id}/options", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Should return all product options for a product")]
    public async Task ShouldReturnAllProductOptionsForAProduct()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();

        var optionType = new ProductOptionType
        {
            Name = $"{_faker.Commerce.ProductMaterial()}-{Guid.NewGuid():N}",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue { Value = _faker.Lorem.Sentence(5) },
                new ProductOptionValue { Value = _faker.Lorem.Sentence(5) }
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var product = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

        var createProductResponse = await Client.PostAsJsonAsync("api/products", product);
        createProductResponse.EnsureSuccessStatusCode();

        var createdProduct = await createProductResponse.DeserializeAsync<ProductResponse>();
        createdProduct.ShouldNotBeNull();

        var command = new CreateProductOptionCommand(createdProduct.Id, optionType.Id);
        await Client.PostAsJsonAsync($"api/products/{createdProduct.Id}/options", command);

        // Act
        var response = await Client.GetAsync($"api/products/{createdProduct.Id}/options");

        // Assert
        response.EnsureSuccessStatusCode();

        var options = await response.DeserializeAsync<List<ProductOptionResponse>>();

        options.ShouldNotBeEmpty();
        options.ShouldAllBe(o => o.ProductId == createdProduct.Id && o.OptionTypeId == optionType.Id);
    }

    [Fact(DisplayName = "Should create a product variant")]
    public async Task ShouldCreateAProductVariant()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();

        var optionType = new ProductOptionType
        {
            Name = $"{_faker.Commerce.ProductMaterial()}-{Guid.NewGuid():N}",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue { Value = _faker.Lorem.Sentence(5) },
                new ProductOptionValue { Value = _faker.Lorem.Sentence(5) }
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var categoryId = await ProductHelper.GetOrCreateCategoryId(Client);

        var product = _productCommandFaker
            .WithCategoryId(categoryId)
            .Generate();

        var createProductResponse = await Client.PostAsJsonAsync("api/products", product);
        createProductResponse.EnsureSuccessStatusCode();

        var createdProduct = await createProductResponse.DeserializeAsync<ProductResponse>();
        createdProduct.ShouldNotBeNull();

        var createProductOptionCommand = new CreateProductOptionCommand(createdProduct.Id, optionType.Id);
        await Client.PostAsJsonAsync($"api/products/{createdProduct.Id}/options", createProductOptionCommand);

        var createProductOptionResponse = await Client.GetAsync($"api/products/{createdProduct.Id}/options");
        createProductOptionResponse.EnsureSuccessStatusCode();

        var options = await createProductOptionResponse.DeserializeAsync<List<ProductOptionResponse>>();

        options.ShouldNotBeEmpty();

        var productOption = options[0].OptionType.Values.First();

        var command = new CreateProductVariantCommand(
            createdProduct.Id,
            $"SKU-{Guid.NewGuid():N}",
            100.00m,
            10,
            [productOption.Id],
            [new CreateProductImage(_faker.Image.PicsumUrl(), _faker.Commerce.ProductName(), 0)]
        );

        // Act
        var response = await Client.PostAsJsonAsync($"api/products/{createdProduct.Id}/variants", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Should return all product variants for a product")]
    public async Task ShouldReturnAllProductVariantsForAProduct()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        // Act
        var response = await Client.GetAsync($"api/products/{product.Id}/variants");

        // Assert
        response.EnsureSuccessStatusCode();

        var variants = await response.DeserializeAsync<List<ProductVariantResponse>>();

        variants.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should update a product variant")]
    public async Task ShouldUpdateAProductVariant()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];

        var command = new UpdateProductVariantCommand(
            variant.Id,
            variant.Price + 10.00m,
            true,
            true
        );

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/variants/{variant.Id}", command);

        // Assert
        response.EnsureSuccessStatusCode();

        var updatedVariant = await response.DeserializeAsync<SimpleProductVariantResponse>();

        updatedVariant.ShouldNotBeNull();
        updatedVariant.Price.ShouldBe(command.Price);
    }

    [Fact(DisplayName = "Should order product variant images")]
    public async Task ShouldOrderProductVariantImages()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);

        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];
        var images = variant.Images;
        images.ShouldNotBeEmpty();

        var reorderedImages = images
            .Select((img, index) => img with { SortOrder = index + 1 })
            .ToImmutableList();
        var command = new OrderVariantImagesCommand(variant.Id, reorderedImages);

        // Act
        var response = await Client.PutAsJsonAsync($"api/products/variants/{variant.Id}/images/order", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "Should return a product variant by id")]
    public async Task ShouldReturnAProductVariantById()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];

        // Act
        var response = await Client.GetAsync($"api/products/variants/{variant.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var returnedVariant = await response.DeserializeAsync<ProductVariantResponse>();
        returnedVariant.ShouldNotBeNull();
        returnedVariant.Id.ShouldBe(variant.Id);
    }

    [Fact(DisplayName = "Should return not found when product variant not exists")]
    public async Task ShouldReturnNotFoundWhenProductVariantNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"api/products/variants/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should add new images to a product variant")]
    public async Task ShouldAddNewImagesToAProductVariant()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];

        var command = new AddVariantImagesCommand(
            variant.Id,
            [
                new CreateProductImage(_faker.Image.PicsumUrl(), "New Image 1", 0),
                new CreateProductImage(_faker.Image.PicsumUrl(), "New Image 2", 1)
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync($"api/products/variants/{variant.Id}/images", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var getVariantResponse = await Client.GetAsync($"api/products/variants/{variant.Id}");
        getVariantResponse.EnsureSuccessStatusCode();

        var updatedVariant = await getVariantResponse.DeserializeAsync<ProductVariantResponse>();
        updatedVariant.ShouldNotBeNull();

        updatedVariant.Images.Count.ShouldBe(variant.Images.Count + 2);
    }

    [Fact(DisplayName = "Should delete a product variant image")]
    public async Task ShouldDeleteAProductVariantImage()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];
        var images = variant.Images;
        images.ShouldNotBeEmpty();

        var imageToDelete = images[0];

        // Act
        var response = await Client.DeleteAsync(
            $"api/products/variants/images/{imageToDelete.Id}?variantId={variant.Id}"
        );

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getVariantResponse = await Client.GetAsync($"api/products/variants/{variant.Id}");
        getVariantResponse.EnsureSuccessStatusCode();

        var updatedVariant = await getVariantResponse.DeserializeAsync<ProductVariantResponse>();
        updatedVariant.ShouldNotBeNull();

        updatedVariant.Images.ShouldNotContain(img => img.Id == imageToDelete.Id);
    }

    [Fact(DisplayName = "Should make a delta inventory adjustment")]
    public async Task ShouldMakeADeltaInventoryAdjustment()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];

        var command = new AdjustInventoryStockCommand(
            "Global restocking adjustment",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant.Id,
                    5,
                    AdjustmentType.Delta,
                    "Restocking"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getVariantAfterAdjustmentResponse = await Client.GetAsync($"api/products/variants/{variant.Id}");
        getVariantAfterAdjustmentResponse.EnsureSuccessStatusCode();

        var updatedVariant = await getVariantAfterAdjustmentResponse.DeserializeAsync<ProductVariantResponse>();
        updatedVariant.ShouldNotBeNull();
        updatedVariant.Stock.ShouldBe(variant.Stock + 5);
    }

    [Fact(DisplayName = "Should make a snapshot inventory adjustment")]
    public async Task ShouldMakeASnapshotInventoryAdjustment()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];

        var command = new AdjustInventoryStockCommand(
            "Global stock correction",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant.Id,
                    20,
                    AdjustmentType.Snapshot,
                    "Correcting stock to 20"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getVariantAfterAdjustmentResponse = await Client.GetAsync($"api/products/variants/{variant.Id}");
        getVariantAfterAdjustmentResponse.EnsureSuccessStatusCode();

        var updatedVariant = await getVariantAfterAdjustmentResponse.DeserializeAsync<ProductVariantResponse>();
        updatedVariant.ShouldNotBeNull();
        updatedVariant.Stock.ShouldBe(20);
    }

    [Fact(DisplayName = "Should return not found when adjusting inventory for non-existent variant")]
    public async Task ShouldReturnNotFoundWhenAdjustingInventoryForNonExistentVariant()
    {
        // Arrange
        const long nonExistentVariantId = long.MaxValue;

        var command = new AdjustInventoryStockCommand(
            "Global stock correction",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    nonExistentVariantId,
                    20,
                    AdjustmentType.Snapshot,
                    "Correcting stock to 20"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Should return error when adjustment results in negative stock")]
    public async Task ShouldReturnErrorWhenAdjustmentResultsInNegativeStock()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();

        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeEmpty();

        var variant = variants[0];
        // Current stock is 10 (based on ProductHelper/Faker defaults usually, but let's assume it's positive)
        // Actually looking at ShouldCreateAProductVariant, stock is 10.
        // Let's try to decrease by 100.

        var command = new AdjustInventoryStockCommand(
            "Global stock correction",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant.Id,
                    -100,
                    AdjustmentType.Delta,
                    "Reducing stock by 100"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should handle multiple adjustments in one request")]
    public async Task ShouldHandleMultipleAdjustmentsInOneRequest()
    {
        // Arrange
        // Create two products/variants
        var product1 = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse1 = await Client.GetAsync($"api/products/{product1.Id}/variants");
        getVariantsResponse1.EnsureSuccessStatusCode();
        var variants1 = await getVariantsResponse1.DeserializeAsync<List<ProductVariantResponse>>();
        variants1.ShouldNotBeNull();
        variants1.ShouldNotBeEmpty();
        var variant1 = variants1[0];

        var product2 = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse2 = await Client.GetAsync($"api/products/{product2.Id}/variants");
        getVariantsResponse2.EnsureSuccessStatusCode();
        var variants2 = await getVariantsResponse2.DeserializeAsync<List<ProductVariantResponse>>();
        variants2.ShouldNotBeNull();
        variants2.ShouldNotBeEmpty();
        var variant2 = variants2[0];

        var command = new AdjustInventoryStockCommand(
            "Bulk adjustment",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant1.Id,
                    5,
                    AdjustmentType.Delta,
                    "Adding 5"
                ),
                new InventoryAdjustmentItem(
                    variant2.Id,
                    20,
                    AdjustmentType.Snapshot,
                    "Setting to 20"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.EnsureSuccessStatusCode();

        var getVariant1Response = await Client.GetAsync($"api/products/variants/{variant1.Id}");
        var updatedVariant1 = await getVariant1Response.DeserializeAsync<ProductVariantResponse>();
        updatedVariant1.ShouldNotBeNull();
        updatedVariant1.Stock.ShouldBe(variant1.Stock + 5);

        var getVariant2Response = await Client.GetAsync($"api/products/variants/{variant2.Id}");
        var updatedVariant2 = await getVariant2Response.DeserializeAsync<ProductVariantResponse>();
        updatedVariant2.ShouldNotBeNull();
        updatedVariant2.Stock.ShouldBe(20);
    }

    [Fact(DisplayName = "Should handle zero quantity change")]
    public async Task ShouldHandleZeroQuantityChange()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();
        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeNull();
        variants.ShouldNotBeEmpty();
        var variant = variants[0];

        var command = new AdjustInventoryStockCommand(
            "Zero adjustment",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant.Id,
                    0,
                    AdjustmentType.Delta,
                    "No change"
                )
            ]
        );

        // Act
        var response = await Client.PostAsJsonAsync("api/inventory/adjustments", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Should return all inventory transactions for a variant")]
    public async Task ShouldReturnAllInventoryTransactions()
    {
        // Arrange
        var product = await ProductHelper.CreateProductWithVariant(Client);
        var getVariantsResponse = await Client.GetAsync($"api/products/{product.Id}/variants");
        getVariantsResponse.EnsureSuccessStatusCode();
        var variants = await getVariantsResponse.DeserializeAsync<List<ProductVariantResponse>>();
        variants.ShouldNotBeNull();
        variants.ShouldNotBeEmpty();
        var variant = variants[0];

        // Make an inventory adjustment to create a transaction
        var adjustmentCommand = new AdjustInventoryStockCommand(
            "Initial stock adjustment",
            "system-user",
            [
                new InventoryAdjustmentItem(
                    variant.Id,
                    10,
                    AdjustmentType.Delta,
                    "Adding initial stock"
                )
            ]
        );

        var adjustmentResponse = await Client.PostAsJsonAsync("api/inventory/adjustments", adjustmentCommand);
        adjustmentResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync($"api/inventory/transactions?variantId={variant.Id}&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var pagedResult = await response.DeserializeAsync<PagedResult<InventoryTransactionResponse>>();

        pagedResult.ShouldNotBeNull();
        pagedResult.Items.ShouldNotBeEmpty();
        pagedResult.Items.ShouldAllBe(t => t.VariantId == variant.Id);
    }
}
