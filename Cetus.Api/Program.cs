using Cetus.Api.Configuration;
using Cetus.Infrastructure.Persistence.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureDatabase();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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
