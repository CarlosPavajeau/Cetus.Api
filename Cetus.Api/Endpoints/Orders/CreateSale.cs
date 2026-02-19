using Application.Abstractions.Messaging;
using Application.Orders;
using Application.Orders.CreateSale;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Customers;
using Domain.Orders;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class CreateSale : IEndpoint
{
    private sealed record Request(
        IReadOnlyList<ItemRequest> Items,
        CustomerRequest Customer,
        OrderChannel Channel,
        PaymentMethod PaymentMethod,
        ShippingRequest? Shipping = null,
        PaymentStatus PaymentStatus = PaymentStatus.Pending
    );

    private sealed record ItemRequest(long VariantId, int Quantity);

    private sealed record CustomerRequest(
        string Phone,
        string Name,
        string? Email = null,
        DocumentType? DocumentType = null,
        string? DocumentNumber = null
    );

    private sealed record ShippingRequest(string Address, Guid? CityId = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("sales", async (
            Request request,
            ICommandHandler<CreateSaleCommand, SimpleOrderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSaleCommand(
                request.Items.Select(i => new CreateSaleItem(i.VariantId, i.Quantity)).ToList(),
                new CreateSaleCustomer(request.Customer.Phone, request.Customer.Name, request.Customer.Email, request.Customer.DocumentType, request.Customer.DocumentNumber),
                request.Channel,
                request.PaymentMethod,
                request.Shipping is null ? null : new CreateSaleShipping(request.Shipping.Address, request.Shipping.CityId),
                request.PaymentStatus
            );

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Orders);
    }
}
