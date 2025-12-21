using Application.Abstractions.Messaging;
using Application.Products.Inventory.Transactions;
using Application.Products.Inventory.Transactions.SearchAll;
using Cetus.Api.Extensions;
using Cetus.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Cetus.Api.Endpoints.Products.Inventory;

internal sealed class SearchAllTransactions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("inventory/transactions", async (
            [AsParameters] SearchInventoryTransactionsQuery query,
            [FromServices]
            IQueryHandler<SearchInventoryTransactionsQuery, PagedResult<InventoryTransactionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Inventory);
    }
}
