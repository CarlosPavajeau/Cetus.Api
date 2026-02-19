using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Products.Options.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Products.Options;

internal sealed class CreateProductOption : IEndpoint
{
    private sealed record Request(long OptionTypeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products/{productId:guid}/options", async (
            Guid productId,
            Request request,
            ICommandHandler<CreateProductOptionCommand> handler,
            HybridCache cache,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductOptionCommand(productId, request.OptionTypeId);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync($"product-options-{tenant.Id}-{productId}", cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Products);
    }
}
