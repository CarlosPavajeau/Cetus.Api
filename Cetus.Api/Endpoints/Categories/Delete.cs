using Application.Abstractions.Messaging;
using Application.Categories.Delete;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;

namespace Cetus.Api.Endpoints.Categories;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("categories/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteCategoryCommand, bool> handler,
            HybridCache cache,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteCategoryCommand(id);

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await cache.RemoveAsync("categories", cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        }).WithTags(Tags.Categories).HasPermission(ClerkPermissions.AppAccess);
    }
}
