using System.Security.Cryptography;
using System.Text;

namespace Cetus.Api.Infrastructure;

/// <summary>
/// Builds cache keys using the hierarchy-with-colons convention:
///   project:service:entity[:segments...]
/// Keys that exceed <see cref="MaxKeyLength"/> characters are SHA-256 hashed while
/// preserving the project:service:entity prefix for observability.
/// Query parameters are sorted deterministically (key ASC, value ASC) before being
/// appended so the same logical request always produces the same cache key regardless
/// of parameter order.
/// </summary>
internal static class CacheKeyBuilder
{
    private const string Project = "cetus";
    private const string Service = "api";
    private const int MaxKeyLength = 200;

    /// <summary>
    /// Builds a cache key from the entity name and optional path segments.
    /// </summary>
    public static string Build(string entity, params string[] segments)
    {
        var parts = new List<string>(segments.Length + 3) { Project, Service, entity };
        parts.AddRange(segments.Where(segment => !string.IsNullOrEmpty(segment)));

        string key = string.Join(":", parts);

        return key.Length > MaxKeyLength
            ? $"{Project}:{Service}:{entity}:{HashKey(key)}"
            : key;
    }

    /// <summary>
    /// Builds a cache key that incorporates URL query parameters.
    /// Parameters are sorted by key then value (case-insensitive) and URL-encoded
    /// before being appended as the last segment.
    /// </summary>
    public static string BuildWithQuery(
        string entity,
        IEnumerable<KeyValuePair<string, string>> queryParams,
        params string[] segments)
    {
        var sortedQuery = queryParams
            .OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Value, StringComparer.OrdinalIgnoreCase)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}");

        string queryString = string.Join("&", sortedQuery);

        if (string.IsNullOrEmpty(queryString))
        {
            return Build(entity, segments);
        }

        var allSegments = new List<string>(segments) { queryString };
        return Build(entity, [.. allSegments]);
    }

    private static string HashKey(string key)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hash).ToUpperInvariant();
    }
}
