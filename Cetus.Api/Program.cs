using System.Reflection;
using Application;
using Cetus.Api;
using Cetus.Api.Extensions;
using Cetus.Api.Realtime;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using DependencyInjection = Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

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

app.UseRateLimiter();

app.MapControllers();

app.MapHub<OrdersHub>("/api/realtime/orders").RequireCors(DependencyInjection.AllowAllCorsPolicy);

await app.RunAsync();

public partial class Program;
