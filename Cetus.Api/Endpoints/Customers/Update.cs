using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Application.Customers.Update;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;

namespace Cetus.Api.Endpoints.Customers;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("customers/{id:guid}", async (
            Guid id,
            UpdateCustomerCommand command,
            ICommandHandler<UpdateCustomerCommand, CustomerResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command with { Id = id }, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Customers);
    }
}
