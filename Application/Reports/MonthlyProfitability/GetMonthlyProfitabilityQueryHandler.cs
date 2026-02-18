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
    private sealed record PeriodRange(
        DateTime SelectedFrom,
        DateTime SelectedTo,
        DateTime ComparisonFrom,
        DateTime ComparisonTo
    );

    public async Task<Result<MonthlyProfitabilityResponse>> Handle(GetMonthlyProfitabilityQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var storeId = tenant.Id;

        var (selectedFrom, selectedTo, comparisonFrom, comparisonTo) =
            ResolveFromPreset(query.Preset, query.Year, query.Month, now);

        var selectedOrdersQuery = BuildOrdersQuery(storeId, selectedFrom, selectedTo, query);
        var comparisonOrdersQuery = BuildOrdersQuery(storeId, comparisonFrom, comparisonTo, query);

        var summary = await GetSummaryAsync(selectedOrdersQuery, cancellationToken);
        var comparisonSummary = await GetSummaryAsync(comparisonOrdersQuery, cancellationToken);
        var previousMonthComparison = ComputeComparison(summary, comparisonSummary);
        var trend = await GetTrendAsync(storeId, query.ExcludeCanceled, query.ExcludeRefunded, now, cancellationToken);
        var productsWithoutCost = await GetProductsWithoutCostAsync(selectedOrdersQuery, cancellationToken);

        return new MonthlyProfitabilityResponse(
            Summary: summary,
            ComparisonSummary: comparisonSummary,
            PreviousMonthComparison: previousMonthComparison,
            Trend: trend,
            ProductsWithoutCost: productsWithoutCost
        );
    }

    private static PeriodRange ResolveFromPreset(PeriodPreset preset, int? year, int? month, DateTime now)
    {
        return preset switch
        {
            PeriodPreset.ThisMonth => ResolveThisMonth(now),
            PeriodPreset.LastMonth => ResolveLastMonth(now),
            PeriodPreset.SpecificMonth => ResolveSpecificMonth(year!.Value, month!.Value),
            _ => ResolveThisMonth(now)
        };
    }

    private static PeriodRange ResolveThisMonth(DateTime now)
    {
        var selectedFrom = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var selectedTo = now.Date.AddDays(1);
        int daysCovered = (selectedTo - selectedFrom).Days;

        var comparisonFrom = selectedFrom.AddMonths(-1);
        var comparisonTo = comparisonFrom.AddDays(daysCovered);

        return new PeriodRange(selectedFrom, selectedTo, comparisonFrom, comparisonTo);
    }

    private static PeriodRange ResolveLastMonth(DateTime now)
    {
        var selectedFrom = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var selectedTo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var comparisonFrom = selectedFrom.AddMonths(-1);

        return new PeriodRange(selectedFrom, selectedTo, comparisonFrom, selectedFrom);
    }

    private static PeriodRange ResolveSpecificMonth(int year, int month)
    {
        var selectedFrom = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var selectedTo = selectedFrom.AddMonths(1);
        var comparisonFrom = selectedFrom.AddMonths(-1);

        return new PeriodRange(selectedFrom, selectedTo, comparisonFrom, selectedFrom);
    }

    private IQueryable<Order> BuildOrdersQuery(
        Guid storeId,
        DateTime from,
        DateTime to,
        GetMonthlyProfitabilityQuery query)
    {
        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to);

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

    private static MonthComparison? ComputeComparison(
        ProfitabilitySummary current,
        ProfitabilitySummary comparison)
    {
        if (comparison is { TotalSales: 0, GrossProfit: 0 })
        {
            return null;
        }

        decimal salesChange = comparison.TotalSales != 0
            ? Math.Round((current.TotalSales - comparison.TotalSales) / Math.Abs(comparison.TotalSales), 2)
            : 0;

        decimal profitChange = comparison.GrossProfit != 0
            ? Math.Round((current.GrossProfit - comparison.GrossProfit) / Math.Abs(comparison.GrossProfit), 2)
            : 0;

        decimal marginChange = Math.Round(current.MarginPercentage - comparison.MarginPercentage, 2);

        return new MonthComparison(salesChange, profitChange, marginChange);
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
