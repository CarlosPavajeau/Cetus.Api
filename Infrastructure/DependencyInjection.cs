﻿using System.Reflection;
using System.Threading.RateLimiting;
using Application.Abstractions.Data;
using Clerk.Net.AspNetCore.Security;
using Domain.Coupons;
using Domain.Orders;
using Domain.Reviews;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Reviews.Jobs;
using Infrastructure.Stores;
using Infrastructure.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Quartz.AspNetCore;
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
            .AddTelemetry(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal()
            .AddCors(configuration)
            .AddEmail(configuration)
            .AddRateLimit()
            .AddCache()
            .AddQuartz();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CetusContext");
        services.AddDbContextPool<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                    connectionString,
                    dbContextOptionsBuilder =>
                    {
                        dbContextOptionsBuilder.MapEnum<OrderStatus>("order_status");
                        dbContextOptionsBuilder.MapEnum<ReviewRequestStatus>("review_request_status");
                        dbContextOptionsBuilder.MapEnum<ProductReviewStatus>("product_review_status");
                        dbContextOptionsBuilder.MapEnum<CouponDiscountType>("coupon_discount_type");
                        dbContextOptionsBuilder.MapEnum<CouponRuleType>("coupon_rule_type");
                    })
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

    private static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "cetus-api";
        var otel = services.AddOpenTelemetry();

        otel.ConfigureResource(resource =>
        {
            resource.AddService(
                serviceName: serviceName,
                serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName
            );
        });

        otel.WithMetrics(metrics => metrics
            .AddMeter(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddFusionCacheInstrumentation()
            .AddNpgsqlInstrumentation()
            .AddRuntimeInstrumentation());

        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddFusionCacheInstrumentation()
                .AddNpgsql()
                .AddQuartzInstrumentation()
                .AddSource(serviceName);
        });

        otel.UseOtlpExporter();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(ClerkAuthenticationDefaults.AuthenticationScheme)
            .AddClerkAuthentication(options =>
            {
                options.Authority = configuration["Clerk:Authority"]!;
                options.AuthorizedParty = configuration["Clerk:AuthorizedParty"]!;
            });

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

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
                        PermitLimit = 100,
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

    private static IServiceCollection AddQuartz(this IServiceCollection services)
    {
        services.AddQuartz(config =>
        {
            var sendPendingReviewRequestsJobKey = new JobKey(SendPendingReviewRequestsJob.Name);
            config.AddJob<SendPendingReviewRequestsJob>(sendPendingReviewRequestsJobKey);

            config.AddTrigger(trigger =>
            {
                trigger
                    .ForJob(sendPendingReviewRequestsJobKey)
                    .WithIdentity($"{sendPendingReviewRequestsJobKey.Name}-trigger")
                    .WithCronSchedule("0 0 9 * * ?") // Every day at 9 AM UTC
                    .WithDescription("Send pending review requests job trigger");
            });
        });

        services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });
        return services;
    }
}
