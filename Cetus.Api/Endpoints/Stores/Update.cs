using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/{id:guid}", async (
            Guid id,
            [FromBody] UpdateStoreCommand command,
            IQueryHandler<UpdateStoreCommand, StoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            if (id != command.Id)
            {
                return Results.BadRequest("Store ID mismatch.");
            }

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync([$"store-id-{id}", $"store-id-{result.Value.Slug}"], cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        });
    }
}
