using Cetus.Api.Middleware;

namespace Cetus.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseTenantResolver(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantResolverMiddleware>();

        return app;
    }
}
