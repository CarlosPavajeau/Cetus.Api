using Application.Abstractions.Messaging;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Inventory.Transactions.SearchAll;

public sealed record SearchInventoryTransactionsQuery(
    int Page = 1,
    int PageSize = 20,
    long? VariantId = null,
    InventoryTransactionType? Type = null,
    DateTime? From = null,
    DateTime? To = null
) : IQuery<PagedResult<InventoryTransactionResponse>>;
