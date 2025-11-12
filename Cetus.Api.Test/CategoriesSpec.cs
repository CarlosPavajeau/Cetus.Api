using System.Net.Http.Json;
using Application.Categories.Create;
using Application.Categories.FindBySlug;
using Application.Categories.SearchAll;
using Application.Categories.Update;
using Bogus;
using Cetus.Api.Test.Shared;
using Shouldly;

namespace Cetus.Api.Test;

public class CategoriesSpec(ApplicationTestCase factory) : ApplicationContextTestCase(factory)
{
    private readonly Faker _faker = new();

    [Fact(DisplayName = "Should create a new category")]
    public async Task ShouldCreateANewCategory()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand(_faker.Commerce.Categories(1)[0]);

        // Act
        var response = await Client.PostAsJsonAsync("api/categories", newCategory);

        // Assert
        response.EnsureSuccessStatusCode();

        var category = await response.DeserializeAsync<CategoryResponse>();

        category.ShouldNotBeNull();
    }

    [Fact(DisplayName = "Should return a list of categories")]
    public async Task ShouldReturnAll()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand(_faker.Commerce.Categories(1)[0]);
        await Client.PostAsJsonAsync("api/categories", newCategory);

        // Act
        var response = await Client.GetAsync("api/categories");

        // Assert
        response.EnsureSuccessStatusCode();

        var categories = await response.DeserializeAsync<List<CategoryResponse>>();

        categories.ShouldNotBeNull().ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "Should update a category")]
    public async Task ShouldUpdate()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand(_faker.Commerce.Categories(1)[0]);
        var createResponse = await Client.PostAsJsonAsync("api/categories", newCategory);
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync("api/categories");
        getResponse.EnsureSuccessStatusCode();

        var categories = await getResponse.DeserializeAsync<List<CategoryResponse>>();

        categories.ShouldNotBeNull().ShouldNotBeEmpty();

        var category = categories[0];

        var updateCategory = new UpdateCategoryCommand(category.Id, _faker.Commerce.Categories(1)[0]);

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"api/categories/{category.Id}", updateCategory);

        // Assert
        updateResponse.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "Should delete a category")]
    public async Task ShouldDelete()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand(_faker.Commerce.Categories(1)[0]);
        var createResponse = await Client.PostAsJsonAsync("api/categories", newCategory);
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync("api/categories");
        getResponse.EnsureSuccessStatusCode();

        var categories = await getResponse.DeserializeAsync<List<CategoryResponse>>();

        categories.ShouldNotBeNull().ShouldNotBeEmpty();

        var category = categories[0];

        // Act
        var deleteResponse = await Client.DeleteAsync($"api/categories/{category.Id}");

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "Should find a category by slug")]
    public async Task ShouldFindBySlug()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand(_faker.Commerce.Categories(1)[0]);
        var createResponse = await Client.PostAsJsonAsync("api/categories", newCategory);
        createResponse.EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync("api/categories");
        getResponse.EnsureSuccessStatusCode();

        var categories = await getResponse.DeserializeAsync<List<CategoryResponse>>();

        categories.ShouldNotBeNull().ShouldNotBeEmpty();

        var category = categories[0];

        // Act
        var findBySlugResponse = await Client.GetAsync($"api/categories/{category.Slug}");

        // Assert
        findBySlugResponse.EnsureSuccessStatusCode();

        var foundCategory = await findBySlugResponse.DeserializeAsync<FindCategoryBySlugResponse>();

        foundCategory.ShouldNotBeNull();
    }
}
