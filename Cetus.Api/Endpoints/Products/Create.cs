using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products;
using Application.Products.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class Create : IEndpoint
{
    private sealed record Request(string Name, string? Description, Guid CategoryId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products", async (
            Request request,
            ICommandHandler<CreateProductCommand, ProductResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand(request.Name, request.Description, request.CategoryId);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Match(Results.Ok, CustomResults.Problem);
            }

            string cacheKey = CacheKeyBuilder.Build("products", "for-sale", tenant.Id.ToString());
            await cache.RemoveAsync(cacheKey, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
