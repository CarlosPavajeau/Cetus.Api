using Cetus.Application.CreateCategory;
using Cetus.Application.SearchAllCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints;

public static class Categories
{
    public static void MapCategories(this WebApplication app)
    {
        app.MapGet("/api/categories",
            async ([FromServices] IMediator mediator) =>
            {
                var categories =
                    await mediator.Send(new SearchAllCategoriesQuery());

                return Results.Ok(categories);
            });

        app.MapPost("/api/categories", async ([FromServices] IMediator mediator,
            [FromBody] CreateCategoryCommand command) =>
        {
            var created = await mediator.Send(command);

            return created ? Results.Created() : Results.BadRequest();
        });
    }
}
