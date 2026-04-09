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
            decimal profit = row.Revenue - row.Costs;
            decimal margin = row.Revenue > 0
                ? Math.Round(profit / row.Revenue, 4)
                : 0m;

            bool isStarProduct = margin >= query.StarMarginThreshold && row.UnitsSold >= query.StarUnitsThreshold;
            bool isProblematic = profit <= 0 || margin <= query.ProblematicMarginThreshold;

            return new ProductProfitabilityItem(
                ProductId: row.ProductId,
                Product: row.Product,
                CategoryId: row.CategoryId,
                Category: row.Category,
                UnitsSold: row.UnitsSold,
                Revenue: row.Revenue,
                Costs: row.Costs,
                Profit: profit,
                MarginPercentage: margin,
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

        return (normalizedSortBy, ascending) switch
        {
            ("MARGIN", true) => query
                .OrderBy(x => x.Revenue == 0 ? 0 : (x.Revenue - x.Costs) / x.Revenue)
                .ThenByDescending(x => x.UnitsSold),
            ("MARGIN", false) => query
                .OrderByDescending(x => x.Revenue == 0 ? 0 : (x.Revenue - x.Costs) / x.Revenue)
                .ThenByDescending(x => x.UnitsSold),
            ("PROFIT", true) => query
                .OrderBy(x => x.Revenue - x.Costs)
                .ThenByDescending(x => x.UnitsSold),
            _ => query
                .OrderByDescending(x => x.Revenue - x.Costs)
                .ThenByDescending(x => x.UnitsSold)
        };
    }

    private sealed record ProductAggregationRow(
        Guid ProductId,
        string Product,
        Guid CategoryId,
        string Category,
        int UnitsSold,
        decimal Revenue,
        decimal Costs
    );
}
