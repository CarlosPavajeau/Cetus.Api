using Cetus.Application.CreateProduct;
using Cetus.Application.SearchAllProducts;
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

        app.MapGet("/api/products", async ([FromServices] IMediator mediator) =>
        {
            var products = await mediator.Send(new SearchAllProductsQuery());
            return Results.Ok(products);
        });
    }
}
