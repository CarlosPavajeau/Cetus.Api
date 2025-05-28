using Application.Abstractions.Messaging;
using Application.Orders.SearchAll;
using Application.Orders.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Orders;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Cancel : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{id:guid}/cancel", async (
            Guid id,
            ICommandHandler<UpdateOrderCommand, OrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateOrderCommand(id, OrderStatus.Canceled);
            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders).HasPermission(Permissions.AppAccess);
    }
}
