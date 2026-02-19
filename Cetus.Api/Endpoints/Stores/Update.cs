using Application.Abstractions.Messaging;
using Application.Stores;
using Application.Stores.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Stores;

internal sealed class Update : IEndpoint
{
    private sealed record Request(
        string Name,
        string? Description,
        string? Address,
        string? Phone,
        string? Email,
        string? CustomDomain
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("stores/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateStoreCommand, StoreResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateStoreCommand(id, request.Name, request.Description, request.Address, request.Phone,
                request.Email, request.CustomDomain);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync([$"store-id-{id}", $"store-id-{result.Value.Slug}"], cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        });
    }
}
