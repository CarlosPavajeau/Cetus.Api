using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Inventory.Transactions.SearchAll;

internal sealed class SearchInventoryTransactionsQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<SearchInventoryTransactionsQuery, PagedResult<InventoryTransactionResponse>>
{
    public async Task<Result<PagedResult<InventoryTransactionResponse>>> Handle(SearchInventoryTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        int page = query.Page <= 0 ? 1 : query.Page;
        int size = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var transactionsQuery = db.InventoryTransactions
            .AsNoTracking()
            .Where(t => t.ProductVariant!.Product!.StoreId == tenant.Id);

        if (query.VariantId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.VariantId == query.VariantId.Value);
        }

        if (query.Type.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Type == query.Type.Value);
        }

        if (query.From.HasValue)
        {
            var from = query.From.Value;
            transactionsQuery = transactionsQuery.Where(t => t.CreatedAt >= from);
        }

        if (query.To.HasValue)
        {
            // Make upper bound exclusive to include whole 'To' day regardless of time component.
            var toExclusive = query.To.Value.Date.AddDays(1);
            transactionsQuery = transactionsQuery.Where(t => t.CreatedAt < toExclusive);
        }

        int total = await transactionsQuery.CountAsync(cancellationToken);

        var items = await transactionsQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(InventoryTransactionResponse.Map)
            .ToListAsync(cancellationToken);

        return PagedResult<InventoryTransactionResponse>.Create(items, page, size, total);
    }
}
