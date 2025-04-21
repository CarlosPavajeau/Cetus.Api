using ZiggyCreatures.Caching.Fusion;

namespace Cetus.Api.Configuration;

public static class Cache
{
    public static WebApplicationBuilder AddCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddFusionCache().AsHybridCache();

        return builder;
    }
}
