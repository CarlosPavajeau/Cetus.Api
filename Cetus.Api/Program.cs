using Cetus.Api.Configuration;
using Cetus.Infrastructure.Persistence.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureDatabase();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/categories", async ([FromServices] CetusDbContext db) =>
{
    try
    {
        var categories = await db.Categories.ToListAsync();
        
        return Results.Ok(categories);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e);
    }
});

app.Run();

public partial class Program
{
}
