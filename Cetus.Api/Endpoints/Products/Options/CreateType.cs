using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options.CreateType;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Options;

internal sealed class CreateType : IEndpoint
{
    private sealed record Request(string Name, IReadOnlyList<string> Values);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/option-types", async (
            Request request,
            ICommandHandler<CreateProductOptionTypeCommand> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductOptionTypeCommand(request.Name, request.Values);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Match(Results.NoContent, CustomResults.Problem);
            }

            string cacheKey = CacheKeyBuilder.Build("products", "option-types", tenant.Id.ToString());
            await cache.RemoveAsync(cacheKey, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
