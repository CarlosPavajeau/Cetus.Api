using Cetus.Infrastructure.Persistence.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel
    .Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel
    .Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.WithProperty("Application", "Cetus.Api")
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSerilog();
builder.Services.AddOpenApi();

builder.Logging.AddSerilog();

builder.Services.AddDbContextPool<CetusDbContext>(options =>
{
    options.UseNpgsql(
            builder.Configuration.GetConnectionString("CetusContext"))
        .UseSnakeCaseNamingConvention();
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/categories", async ([FromServices] CetusDbContext db) =>
{
    var categories = await db.Categories.ToListAsync();
    return Results.Ok(categories);
});

app.Run();
