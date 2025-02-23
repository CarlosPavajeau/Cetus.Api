using System.Net.Http.Json;
using Cetus.Api.Test.Shared;
using Cetus.Application.CreateCategory;
using Shouldly;

namespace Cetus.Api.Test;

public class Categories(ApplicationTestCase factory)
    : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should create a new category")]
    public async Task ShouldCreateANewCategory()
    {
        // Arrange 
        var newCategory = new CreateCategoryCommand("Test");

        // Act
        var response =
            await Client.PostAsJsonAsync("api/categories", newCategory);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "Should return a list of categories")]
    public async Task ShouldReturnAll()
    {
        // Act
        var response = await Client.GetAsync("api/categories");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.ShouldNotBeEmpty("content != null");
    }
}
