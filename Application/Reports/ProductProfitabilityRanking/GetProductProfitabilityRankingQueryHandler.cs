using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.ProductProfitabilityRanking;

internal sealed class GetProductProfitabilityRankingQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<GetProductProfitabilityRankingQuery, IReadOnlyList<ProductProfitabilityItem>>
{
    public async Task<Result<IReadOnlyList<ProductProfitabilityItem>>> Handle(
        GetProductProfitabilityRankingQuery query,
        CancellationToken cancellationToken)
    {
        var ordersQuery = BuildOrdersQuery(query);

        var productRows = db.OrderItems
            .AsNoTracking()
            .Where(oi => ordersQuery.Select(o => o.Id).Contains(oi.OrderId))
            .Where(oi => oi.ProductVariant != null && oi.ProductVariant.Product != null)
            .Select(oi => new
            {
                oi.ProductVariant!.ProductId,
                ProductName = oi.ProductVariant.Product!.Name,
                oi.ProductVariant.Product.CategoryId,
                CategoryName = oi.ProductVariant.Product.Category != null
                    ? oi.ProductVariant.Product.Category.Name
                    : string.Empty,
                UnitsSold = oi.Quantity,
                Revenue = oi.Price * oi.Quantity,
                Costs = (oi.CostPrice ?? 0) * oi.Quantity
            });

        if (query.CategoryId.HasValue)
        {
            var categoryId = query.CategoryId.Value;
            productRows = productRows.Where(x => x.CategoryId == categoryId);
        }

        var grouped = productRows
            .GroupBy(x => new { x.ProductId, x.ProductName, x.CategoryId, x.CategoryName })
            .Select(g => new ProductAggregationRow(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(x => x.UnitsSold),
                g.Sum(x => x.Revenue),
                g.Sum(x => x.Costs)
            ));

        grouped = ApplySorting(grouped, query.ResolvedSortBy, query.ResolvedSortDirection);

        var data = await grouped.ToListAsync(cancellationToken);
        var items = data.Select(row =>
        {
            bool isStarProduct = row.Margin >= query.StarMarginThreshold && row.UnitsSold >= query.StarUnitsThreshold;
            bool isProblematic = row.Profit <= 0 || row.Margin <= query.ProblematicMarginThreshold;

            return new ProductProfitabilityItem(
                ProductId: row.ProductId,
                Product: row.Product,
                CategoryId: row.CategoryId,
                Category: row.Category,
                UnitsSold: row.UnitsSold,
                Revenue: row.Revenue,
                Costs: row.Costs,
                Profit: row.Profit,
                MarginPercentage: row.Margin,
                IsStarProduct: isStarProduct,
                IsProblematic: isProblematic
            );
        }).ToList();

        return items;
    }

    private IQueryable<Order> BuildOrdersQuery(GetProductProfitabilityRankingQuery query)
    {
        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == tenant.Id);

        if (query.From.HasValue)
        {
            var from = query.From.Value.Date;
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= from);
        }

        if (query.To.HasValue)
        {
            var toExclusive = query.To.Value.Date.AddDays(1);
            ordersQuery = ordersQuery.Where(o => o.CreatedAt < toExclusive);
        }

        if (query.ExcludeCanceled)
        {
            ordersQuery = ordersQuery
                .Where(o => o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Returned);
        }

        if (query.ExcludeRefunded)
        {
            ordersQuery = ordersQuery
                .Where(o => o.PaymentStatus != PaymentStatus.Refunded);
        }

        return ordersQuery;
    }

    private static IQueryable<ProductAggregationRow> ApplySorting(
        IQueryable<ProductAggregationRow> query,
        string sortBy,
        string sortDirection)
    {
        bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        string normalizedSortBy = sortBy.Trim().ToUpperInvariant();
        var orderedQuery = (normalizedSortBy, ascending) switch
        {
            ("MARGIN", true) => query.OrderBy(x => x.Margin),
            ("MARGIN", false) => query.OrderByDescending(x => x.Margin),
            ("PROFIT", true) => query.OrderBy(x => x.Profit),
            _ => query.OrderByDescending(x => x.Profit)
        };

        return orderedQuery.ThenByDescending(x => x.UnitsSold);
    }

    private sealed record ProductAggregationRow(
        Guid ProductId,
        string Product,
        Guid CategoryId,
        string Category,
        int UnitsSold,
        decimal Revenue,
        decimal Costs
    )
    {
        public decimal Profit => Revenue - Costs;

        public decimal Margin => Revenue > 0
            ? (Revenue - Costs) / Revenue
            : 0m;
    }
}
