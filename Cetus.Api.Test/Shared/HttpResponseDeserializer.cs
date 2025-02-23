using System.Text.Json;

namespace Cetus.Api.Test.Shared;

public static class HttpResponseDeserializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public static async Task<T?> DeserializeAsync<T>(
        this HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseContent =
            await httpResponseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(responseContent,
            JsonSerializerOptions);
    }
}
