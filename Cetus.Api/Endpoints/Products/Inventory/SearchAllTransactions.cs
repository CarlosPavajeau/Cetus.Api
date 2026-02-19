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
    private sealed record Request(
        int Page = 1,
        int PageSize = 20,
        long? VariantId = null,
        string[]? Types = null,
        DateTime? From = null,
        DateTime? To = null
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("inventory/transactions", async (
            [AsParameters] Request request,
            [FromServices]
            IQueryHandler<SearchInventoryTransactionsQuery, PagedResult<InventoryTransactionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchInventoryTransactionsQuery(request.Page, request.PageSize, request.VariantId,
                request.Types, request.From, request.To);
            var result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags(Tags.Inventory);
    }
}
