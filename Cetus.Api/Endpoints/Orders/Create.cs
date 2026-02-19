using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.Create;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Cetus.Api.Realtime;
using Domain.Customers;
using Microsoft.AspNetCore.SignalR;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class Create : IEndpoint
{
    private sealed record Request(
        IReadOnlyList<ItemRequest> Items,
        CustomerRequest Customer,
        ShippingRequest Shipping
    );

    private sealed record ItemRequest(long VariantId, int Quantity);

    private sealed record CustomerRequest(
        string Phone,
        string Name,
        string? Email = null,
        DocumentType? DocumentType = null,
        string? DocumentNumber = null
    );

    private sealed record ShippingRequest(string Address, Guid CityId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders", async (
            Request request,
            ICommandHandler<CreateOrderCommand, SimpleOrderResponse> handler,
            IHubContext<OrdersHub, IOrdersClient> hub,
            ITenantContext tenant,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderCommand(
                request.Items.Select(i => new CreateOrderItem(i.VariantId, i.Quantity)).ToList(),
                new CreateOrderCustomer(request.Customer.Phone, request.Customer.Name, request.Customer.Email, request.Customer.DocumentType, request.Customer.DocumentNumber),
                new CreateOrderShipping(request.Shipping.Address, request.Shipping.CityId)
            );

            var result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                await hub.Clients.Group(tenant.Id.ToString()).ReceiveCreatedOrder(result.Value);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
