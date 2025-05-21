using System.Threading.RateLimiting;
using Application.Abstractions.Data;
using Clerk.Net.AspNetCore.Security;
using Domain.Orders;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using SharedKernel;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure;

public static class DependencyInjection
{
    public const string AllowAllCorsPolicy = "AllowAll";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthentication(configuration)
            .AddCors(configuration)
            .AddEmail(configuration)
            .AddRateLimit()
            .AddCache();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<DomainEventsDispatcher>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CetusContext");
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    dbContextOptionsBuilder => { dbContextOptionsBuilder.MapEnum<OrderStatus>("order_status"); })
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("CetusContext")!);

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(ClerkAuthenticationDefaults.AuthenticationScheme)
            .AddClerkAuthentication(options =>
            {
                options.Authority = configuration["Clerk:Authority"]!;
                options.AuthorizedParty = configuration["Clerk:AuthorizedParty"]!;
            });

        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AllowAllCorsPolicy, policy =>
            {
                var allowedOrigin = configuration["AllowedOrigin"]!;

                policy
                    .WithOrigins(allowedOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.AddHttpClient<ResendClient>();

        services.Configure<ResendClientOptions>(options => { options.ApiToken = configuration["Resend:ApiToken"]!; });

        services.AddTransient<IResend, ResendClient>();

        return services;
    }

    private static IServiceCollection AddRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("fixed", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1)
                    })
            );
        });

        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddFusionCache().AsHybridCache();

        return services;
    }
}
