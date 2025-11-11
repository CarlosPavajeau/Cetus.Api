using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.Find;
using Infrastructure.Stores;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IQueryHandler<FindStoreQuery, SimpleStoreResponse> handler,
        TenantContext tenantContext,
        ILogger<TenantResolverMiddleware> logger,
        HybridCache cache)
    {
        string? domain = null;
        if (context.Request.Headers.TryGetValue("Referer", out var originValues))
        {
            var origin = originValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            {
                domain = originUri.Host;
            }
        }

        if (string.IsNullOrEmpty(domain) && context.Request.Headers.TryGetValue("Origin", out var refererValues))
        {
            var referer = refererValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                domain = refererUri.Host;
            }
        }

        if (string.IsNullOrEmpty(domain))
        {
            domain = context.Request.Host.Host;
        }

        string? slug = null;
        if (context.Request.Query.TryGetValue("store", out var storeSlug))
        {
            slug = storeSlug.ToString();
        }

        var cacheKey = BuildCacheKey(domain, slug);

        logger.LogInformation("Try to find store for domain {Domain} and slug {Slug}", domain, slug);

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await handler.Handle(new FindStoreQuery(domain, slug), token),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(5), LocalCacheExpiration = TimeSpan.FromHours(5)
            },
            cancellationToken: context.RequestAborted
        );

        if (result.IsSuccess)
        {
            logger.LogInformation("Found store for domain {Domain} and slug {Slug}", domain, slug);
            var store = result.Value;
            tenantContext.Id = store.Id;

            context.Response.Headers.TryAdd("X-Tenant-Id", store.Id.ToString());
            context.Response.Headers.TryAdd("X-Tenant-Domain", store.CustomDomain);
            context.Response.Headers.TryAdd("X-Tenant-Name", store.Name);
        }
        else
        {
            logger.LogWarning("Could not found store for domain {Domain} and slug {Slug}", domain, slug);
        }

        await next(context);
    }

    private static string BuildCacheKey(string? customDomain, string? slug)
    {
        return $"tenant:{customDomain ?? "null"}:{slug ?? "null"}";
    }
}
