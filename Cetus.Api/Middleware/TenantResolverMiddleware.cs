using Application.Abstractions.Data;
using Infrastructure.Stores;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Api.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IApplicationDbContext db, TenantContext tenantContext)
    {
        string? domain = null;

        if (context.Request.Headers.TryGetValue("Origin", out var originValues))
        {
            var origin = originValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            {
                domain = originUri.Host;
            }
        }

        if (string.IsNullOrEmpty(domain) && context.Request.Headers.TryGetValue("Referer", out var refererValues))
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

        var foundStore = await db.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CustomDomain == domain);

        if (foundStore is null && context.Request.Query.TryGetValue("store", out var storeSlug))
        {
            foundStore = await db.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Slug == storeSlug.ToString());
        }

        if (foundStore is not null)
        {
            tenantContext.Id = foundStore.Id;
            context.Response.Headers.TryAdd("X-Tenant-Id", foundStore.Id.ToString());
            context.Response.Headers.TryAdd("X-Tenant-Domain", foundStore.CustomDomain);
        }

        await next(context);
    }
}
