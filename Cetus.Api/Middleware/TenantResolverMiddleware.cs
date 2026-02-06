using Infrastructure.Stores;

namespace Cetus.Api.Middleware;

public class TenantResolverMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        TenantContext tenantContext,
        ILogger<TenantResolverMiddleware> logger)
    {
        if (!context.Request.Headers.TryGetValue("X-Current-Store-Id", out var storeIdValues)
            || !Guid.TryParse(storeIdValues.FirstOrDefault(), out var storeId))
        {
            logger.LogInformation("No valid X-Current-Store-Id header provided to resolve tenant.");
            await next(context);
            return;
        }

        tenantContext.Id = storeId;

        await next(context);
    }
}
