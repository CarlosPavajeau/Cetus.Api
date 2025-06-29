using Application.Abstractions.Data;
using Infrastructure.Stores;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Api.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IApplicationDbContext db, TenantContext tenantContext)
    {
        var host = context.Request.Host.Host;
        var foundStore = await db.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CustomDomain == host);

        if (foundStore is null && context.Request.Query.TryGetValue("store", out var storeSlug))
        {
            foundStore = await db.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Slug == storeSlug.ToString());
        }

        if (foundStore is not null)
        {
            tenantContext.Id = foundStore.Id;
        }

        await next(context);
    }
}
