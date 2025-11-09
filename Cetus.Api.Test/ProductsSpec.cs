using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using Application.Abstractions.Data;
using Application.Products;
using Application.Products.Create;
using Application.Products.Options;
using Application.Products.Options.Create;
using Application.Products.Options.CreateType;
using Application.Products.TopSelling;
using Application.Products.Update;
using Application.Products.Variants;
using Application.Products.Variants.Create;
using Application.Products.Variants.OrderImages;
using Application.Products.Variants.Update;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Api.Test.Shared.Fakers;
using Cetus.Api.Test.Shared.Helpers;
using Domain.Categories;
using Domain.Products;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cetus.Api.Test;

public class ProductsSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Guid categoryId = Guid.Parse("f97957e9-d820-4858-ac26-b5d03d658370");
    private readonly CreateProductCommandFaker _productCommandFaker = new();
    private readonly Faker _faker = new();

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

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 1",
                Description = "Description 1",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-1",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Description = "Description 2",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-2",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
            }
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Categories.AddAsync(category);
        await db.Products.AddRangeAsync(products);

        await db.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("api/products/for-sale");

        // Assert
        response.EnsureSuccessStatusCode();

        var productsResponse = await response.DeserializeAsync<IEnumerable<SimpleProductForSaleResponse>>();

        productsResponse.ShouldNotBeEmpty();
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

        var tenant = Services.GetRequiredService<ITenantContext>();

        // Create featured products directly in the database
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 1",
                Description = "Description 1",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-1",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Description = "Description 2",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-2",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
            }
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Categories.AddAsync(category);
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
                Name = "Featured Product 1",
                Description = "Description 1",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-1",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 10
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Description = "Description 2",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-2",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true,
                        SalesCount = 50
                    }
                }
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
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-1",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Featured Product 2",
                Description = "Description 2",
                Enabled = true,
                CategoryId = category.Id,
                StoreId = tenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Variants = new List<ProductVariant>
                {
                    new()
                    {
                        Sku = "featured-2",
                        Price = 100,
                        Stock = 10,
                        Enabled = true,
                        Featured = true
                    }
                }
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

        var tenant = Services.GetRequiredService<ITenantContext>();

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Featured Product 1",
            Description = "Description 1",
            Enabled = true,
            CategoryId = category.Id,
            StoreId = tenant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Variants = new List<ProductVariant>
            {
                new()
                {
                    Sku = "featured-1",
                    Price = 100,
                    Stock = 10,
                    Enabled = true,
                    Featured = true
                }
            }
        };

        var db = Services.GetRequiredService<IApplicationDbContext>();

        await db.Categories.AddAsync(category);
        await db.Products.AddAsync(newProduct);

        await db.SaveChangesAsync();

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

    [Fact(DisplayName = "Should create a product option")]
    public async Task ShouldCreateProductOption()
    {
        // Arrange
        var db = Services.GetRequiredService<IApplicationDbContext>();
        var tenant = Services.GetRequiredService<ITenantContext>();
        var optionType = new ProductOptionType
        {
            Name = "Color",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue
                {
                    Value = "Red"
                }
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var product = _productCommandFaker.Generate();
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
            Name = "Color",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue {Value = "Red"},
                new ProductOptionValue {Value = "Blue"}
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var product = _productCommandFaker.Generate();
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
            Name = "Color",
            StoreId = tenant.Id,
            ProductOptionValues =
            [
                new ProductOptionValue {Value = "Red"},
                new ProductOptionValue {Value = "Blue"}
            ]
        };

        await db.ProductOptionTypes.AddAsync(optionType);
        await db.SaveChangesAsync();

        var product = _productCommandFaker.Generate();
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
            variant.Stock + 5,
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
        updatedVariant.Stock.ShouldBe(command.Stock);
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
            .Select((img, index) => img with {SortOrder = index + 1})
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
}
