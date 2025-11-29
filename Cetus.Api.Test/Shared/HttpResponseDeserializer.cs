using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cetus.Api.Test.Shared;

public static class HttpResponseDeserializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)}
        };

    public static async Task<T?> DeserializeAsync<T>(
        this HttpResponseMessage httpResponseMessage)
    {
        httpResponseMessage.EnsureSuccessStatusCode();

        string responseContent =
            await httpResponseMessage.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(responseContent,
            JsonSerializerOptions);
    }
}
