using Cetus.Application.CreateProduct;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints;

public static class Products
{
    public static void MapProducts(this WebApplication app)
    {
        app.MapPost("/api/products", async ([FromServices] IMediator mediator,
            [FromBody] CreateProductCommand command) =>
        {
            var result = await mediator.Send(command);

            return Results.Created($"/api/products/{result.Id}", result);
        });
    }
}
