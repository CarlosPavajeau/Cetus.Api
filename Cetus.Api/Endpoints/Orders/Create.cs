using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.Create;
using Application.Orders.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Cetus.Api.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
            [FromBody] CreateOrderCommand command,
            ICommandHandler<CreateOrderCommand, OrderResponse> handler,
            IHubContext<OrdersHub, IOrdersClient> hub,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await hub.Clients.Group(tenant.Id.ToString()).ReceiveCreatedOrder(result.Value);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
