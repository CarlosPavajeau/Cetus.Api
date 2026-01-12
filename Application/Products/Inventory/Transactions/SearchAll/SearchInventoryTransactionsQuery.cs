using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Products.Inventory.Transactions.SearchAll;

public sealed record SearchInventoryTransactionsQuery(
    int Page = 1,
    int PageSize = 20,
    long? VariantId = null,
    string[]? Types = null,
    DateTime? From = null,
    DateTime? To = null
) : IQuery<PagedResult<InventoryTransactionResponse>>;
