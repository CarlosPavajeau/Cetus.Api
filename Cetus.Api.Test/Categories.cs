using Cetus.Api.Test.Shared;
using Shouldly;

namespace Cetus.Api.Test;

public class Categories(ApplicationTestCase factory)
    : ApplicationContextTestCase(factory)
{
    [Fact(DisplayName = "Should return a list of categories")]
    public async Task GetAll()
    {
        // Act
        var response = await Client.GetAsync("api/categories");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        content.ShouldNotBeEmpty("content != null");
    }
}
