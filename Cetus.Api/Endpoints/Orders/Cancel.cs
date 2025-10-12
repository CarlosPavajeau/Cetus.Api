using Application.Abstractions.Messaging;
using Application.Orders.Cancel;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{id:guid}/cancel", async (
            Guid id,
            ICommandHandler<CancelOrderCommand, OrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CancelOrderCommand(id);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders).HasPermission(ClerkPermissions.AppAccess);
    }
}
