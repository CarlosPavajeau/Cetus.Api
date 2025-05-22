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
            
            if (result.IsSuccess)
            {
                await cache.RemoveAsync($"product-{command.Id}", cancellationToken);
                await cache.RemoveAsync("products-for-sale", cancellationToken);
            }
            
            return result.Match(Results.Ok, CustomResults.Problem);
        });
    }
}
