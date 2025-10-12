using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.Deliver;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Deliver : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{id:guid}/deliver", async (
            Guid id,
            ICommandHandler<DeliverOrderCommand, SimpleOrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeliverOrderCommand(id);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders).HasPermission(ClerkPermissions.AppAccess);
    }
}
