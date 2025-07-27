using Application.Abstractions.Messaging;
using Application.Orders.CreatePayment;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Cetus.Api.Endpoints.Orders;

internal sealed class CreatePayment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("orders/{id:guid}/payments", async (
            [FromRoute] Guid id,
            ICommandHandler<CreateOrderPaymentCommand, string> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateOrderPaymentCommand(id);

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).AllowAnonymous().WithTags(Tags.Orders);
    }
}
