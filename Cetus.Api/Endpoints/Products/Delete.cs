using Application.Abstractions.Messaging;
using Application.Products.Delete;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("products/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteProductCommand, bool> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Match(Results.Ok, CustomResults.Problem);
            }

            await cache.RemoveAsync(CacheKeyBuilder.Build("products", command.Id.ToString()), cancellationToken);
            await cache.RemoveAsync(CacheKeyBuilder.Build("products", "for-sale"), cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
