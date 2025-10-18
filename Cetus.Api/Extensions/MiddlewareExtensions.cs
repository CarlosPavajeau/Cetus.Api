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
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"),
            branch => { branch.UseMiddleware<TenantResolverMiddleware>(); });

        return app;
    }
}
