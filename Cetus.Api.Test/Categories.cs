using Cetus.Api.Test.Shared;

namespace Cetus.Api.Test;

public class Categories : ApplicationContextTestCase
{
    public Categories(ApplicationTestCase factory) : base(
        factory)
    {
    }

    [Fact(DisplayName = "Should return a list of categories")]
    public async Task GetAll()
    {
        // Act
        var response = await Client.GetAsync("api/categories");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        Assert.NotEmpty(content);
    }
}
