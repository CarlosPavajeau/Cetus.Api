using Cetus;
using Cetus.Api.Configuration;
using Cetus.Application.SearchAllCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureDatabase();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
    configuration.RegisterServicesFromAssembly(typeof(CetusAssemblyHelper)
        .Assembly);
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/categories", async ([FromServices] IMediator mediator) =>
{
    var categories = await mediator.Send(new SearchAllCategoriesQuery());

    return Results.Ok(categories);
});

app.Run();

public partial class Program
{
}
