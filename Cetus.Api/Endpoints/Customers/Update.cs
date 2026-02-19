using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Application.Customers.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Domain.Customers;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class Update : IEndpoint
{
    private sealed record Request(
        DocumentType? DocumentType,
        string? DocumentNumber,
        string Name,
        string? Email,
        string Phone,
        string? Address
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("customers/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateCustomerCommand, CustomerResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCustomerCommand(
                id,
                request.DocumentType,
                request.DocumentNumber,
                request.Name,
                request.Email,
                request.Phone,
                request.Address
            );

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Customers);
    }
}
