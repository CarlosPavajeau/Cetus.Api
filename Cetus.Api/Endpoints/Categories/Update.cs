using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Categories.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Categories;

internal sealed class Update : IEndpoint
{
    private sealed record Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("categories/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateCategoryCommand, bool> handler,
            HybridCache cache,
            ITenantContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand(id, request.Name);
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync($"categories-{context.Id}", cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Categories);
    }
}
