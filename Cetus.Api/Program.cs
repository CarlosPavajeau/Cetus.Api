using System.Reflection;
using Application;
using Cetus.Api;
using Cetus.Api.Extensions;
using Cetus.Api.Realtime;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using DependencyInjection = Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);

    loggerConfig.WriteTo.OpenTelemetry(config =>
    {
        config.Endpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4318/v1/logs";
        config.Protocol = OtlpProtocol.HttpProtobuf;
        config.Headers = new Dictionary<string, string>
        {
            {"api-key", context.Configuration["OTEL_EXPORTER_OTLP_API_KEY"] ?? string.Empty}
        };

        config.ResourceAttributes.Add("service.name", context.Configuration["OTEL_SERVICE_NAME"] ?? "cetus-api");
        config.ResourceAttributes.Add("service.version",
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");
        config.ResourceAttributes.Add("service.instance.id", Environment.MachineName);
    });
});

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.ParseStateValues = true;
    options.IncludeScopes = true;
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

var apiGroup = app.MapGroup("/api")
    .RequireAuthorization()
    .RequireCors(DependencyInjection.AllowAllCorsPolicy)
    .RequireRateLimiting("fixed");

app.MapEndpoints(apiGroup);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors(DependencyInjection.AllowAllCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsProduction())
{
    app.UseRateLimiter();
    app.UseTenantResolver();
}

app.MapControllers();

app.MapHub<OrdersHub>("/api/realtime/orders").RequireCors(DependencyInjection.AllowAllCorsPolicy);

await app.RunAsync();

public partial class Program;
