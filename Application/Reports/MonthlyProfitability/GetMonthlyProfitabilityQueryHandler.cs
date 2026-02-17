using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.MonthlyProfitability;

internal sealed class GetMonthlyProfitabilityQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetMonthlyProfitabilityQuery, MonthlyProfitabilityResponse>
{
    public async Task<Result<MonthlyProfitabilityResponse>> Handle(GetMonthlyProfitabilityQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var from = query.From?.Date ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = query.To?.Date ?? now.Date.AddDays(1);
        var storeId = tenant.Id;

        var baseOrdersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to);

        if (query.ExcludeCanceled)
        {
            baseOrdersQuery = baseOrdersQuery
                .Where(o => o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Returned);
        }

        if (query.ExcludeRefunded)
        {
            baseOrdersQuery = baseOrdersQuery
                .Where(o => o.PaymentStatus != PaymentStatus.Refunded);
        }

        var summary = await GetSummaryAsync(baseOrdersQuery, cancellationToken);
        var trend = await GetTrendAsync(storeId, query.ExcludeCanceled, query.ExcludeRefunded, now, cancellationToken);
        var previousMonthComparison = GetPreviousMonthComparison(trend, now);
        var productsWithoutCost = await GetProductsWithoutCostAsync(baseOrdersQuery, cancellationToken);

        return new MonthlyProfitabilityResponse(
            Summary: summary,
            PreviousMonthComparison: previousMonthComparison,
            Trend: trend,
            ProductsWithoutCost: productsWithoutCost
        );
    }

    private async Task<ProfitabilitySummary> GetSummaryAsync(
        IQueryable<Order> ordersQuery,
        CancellationToken cancellationToken)
    {
        var validOrderIds = ordersQuery.Select(o => o.Id);

        var metrics = await db.OrderItems
            .AsNoTracking()
            .Where(oi => validOrderIds.Contains(oi.OrderId))
            .GroupBy(_ => true)
            .Select(g => new
            {
                TotalSales = g.Sum(x => x.Price * x.Quantity),
                TotalCost = g.Sum(x => (x.CostPrice ?? 0) * x.Quantity)
            })
            .FirstOrDefaultAsync(cancellationToken);

        decimal totalSales = metrics?.TotalSales ?? 0;
        decimal totalCost = metrics?.TotalCost ?? 0;
        decimal grossProfit = totalSales - totalCost;
        decimal marginPercentage = totalSales > 0
            ? Math.Round(grossProfit / totalSales * 100, 2)
            : 0;

        return new ProfitabilitySummary(totalSales, totalCost, grossProfit, marginPercentage);
    }

    private async Task<IReadOnlyList<MonthlyTrend>> GetTrendAsync(
        Guid storeId,
        bool excludeCanceled,
        bool excludeRefunded,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var sixMonthsAgo = now.Date.AddMonths(-5);

        var trendOrdersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Where(o => o.CreatedAt >= sixMonthsAgo)
            .Where(o => o.CreatedAt < now);

        if (excludeCanceled)
        {
            trendOrdersQuery = trendOrdersQuery
                .Where(o => o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Returned);
        }

        if (excludeRefunded)
        {
            trendOrdersQuery = trendOrdersQuery
                .Where(o => o.PaymentStatus != PaymentStatus.Refunded);
        }

        var trendOrderIds = trendOrdersQuery.Select(o => new { o.Id, o.CreatedAt });

        var monthlyData = await db.OrderItems
            .AsNoTracking()
            .Join(
                trendOrderIds,
                oi => oi.OrderId,
                o => o.Id,
                (oi, o) => new { oi.Price, oi.Quantity, oi.CostPrice, o.CreatedAt })
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                TotalSales = g.Sum(x => x.Price * x.Quantity),
                TotalCost = g.Sum(x => (x.CostPrice ?? 0) * x.Quantity)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return
        [
            .. monthlyData.Select(m =>
            {
                decimal grossProfit = m.TotalSales - m.TotalCost;
                decimal marginPercentage = m.TotalSales > 0
                    ? Math.Round(grossProfit / m.TotalSales * 100, 2)
                    : 0;

                return new MonthlyTrend(m.Year, m.Month, m.TotalSales, m.TotalCost, grossProfit, marginPercentage);
            })
        ];
    }

    private static MonthComparison? GetPreviousMonthComparison(IReadOnlyList<MonthlyTrend> trend, DateTime now)
    {
        if (trend.Count < 2)
        {
            return null;
        }

        var currentMonth = trend.FirstOrDefault(t => t.Year == now.Year && t.Month == now.Month);
        var previousDate = now.AddMonths(-1);
        var previousMonth = trend.FirstOrDefault(t => t.Year == previousDate.Year && t.Month == previousDate.Month);

        if (currentMonth is null || previousMonth is null)
        {
            return null;
        }

        decimal salesChange = previousMonth.TotalSales != 0
            ? Math.Round(
                (currentMonth.TotalSales - previousMonth.TotalSales) / Math.Abs(previousMonth.TotalSales) * 100, 2)
            : 0;

        decimal profitChange = previousMonth.GrossProfit != 0
            ? Math.Round(
                (currentMonth.GrossProfit - previousMonth.GrossProfit) / Math.Abs(previousMonth.GrossProfit) * 100, 2)
            : 0;

        decimal marginChange = Math.Round(currentMonth.MarginPercentage - previousMonth.MarginPercentage, 2);

        return new MonthComparison(salesChange, profitChange, marginChange);
    }

    private async Task<IReadOnlyList<ProductCostWarning>> GetProductsWithoutCostAsync(
        IQueryable<Order> ordersQuery,
        CancellationToken cancellationToken)
    {
        var validOrderIds = ordersQuery.Select(o => o.Id);

        var warnings = await db.OrderItems
            .AsNoTracking()
            .Where(oi => validOrderIds.Contains(oi.OrderId))
            .Where(oi => oi.CostPrice == null)
            .Join(
                db.ProductVariants.AsNoTracking(),
                oi => oi.VariantId,
                pv => pv.Id,
                (oi, pv) => new { pv.ProductId, oi.ProductName, pv.Id, pv.Sku })
            .Distinct()
            .ToListAsync(cancellationToken);

        return
        [
            .. warnings.Select(w => new ProductCostWarning(w.ProductId, w.ProductName, w.Id, w.Sku))
        ];
    }
}
