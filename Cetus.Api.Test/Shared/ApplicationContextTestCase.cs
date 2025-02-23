namespace Cetus.Api.Test.Shared;

public class ApplicationContextTestCase : IClassFixture<ApplicationTestCase>
{
    private readonly ApplicationTestCase _factory;
    protected readonly HttpClient Client;

    public ApplicationContextTestCase(ApplicationTestCase factory)
    {
        _factory = factory;
        Client = CreateHttpClient();
    }

    private HttpClient CreateHttpClient()
    {
        return _factory.CreateDefaultClient();
    }
}
