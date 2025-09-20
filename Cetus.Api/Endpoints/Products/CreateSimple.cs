using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.CreateSimple;
using Application.Products.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products;

internal sealed class CreateSimple : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/simple", async (
            CreateSimpleProductCommand command,
            ICommandHandler<CreateSimpleProductCommand, ProductResponse> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync($"products-for-sale-{tenant.Id}", cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
