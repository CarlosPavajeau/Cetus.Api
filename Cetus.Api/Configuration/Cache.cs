using ZiggyCreatures.Caching.Fusion;

namespace Cetus.Api.Configuration;

public static class Cache
{
    public static void ConfigureCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddFusionCache().AsHybridCache();
    }
}
