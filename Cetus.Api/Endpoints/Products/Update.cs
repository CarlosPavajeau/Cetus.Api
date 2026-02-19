using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class Update : IEndpoint
{
    private sealed record Request(string Name, string? Description, Guid CategoryId, bool Enabled);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateProductCommand, ProductResponse> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.CategoryId,
                request.Enabled);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Match(Results.Ok, CustomResults.Problem);
            }

            string cacheKey = CacheKeyBuilder.Build("products", id.ToString());
            await cache.RemoveAsync(cacheKey, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
