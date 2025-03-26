using System.Net.Http.Json;
using Bogus;
using Cetus.Api.Test.Shared;
using Cetus.Categories.Application.Create;
using Cetus.Categories.Application.SearchAll;
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
}
