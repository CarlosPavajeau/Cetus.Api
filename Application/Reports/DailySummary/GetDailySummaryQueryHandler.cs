using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reports.DailySummary;

internal sealed class GetDailySummaryQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetDailySummaryQuery, DailySummaryResponse>
{
    public async Task<Result<DailySummaryResponse>> Handle(GetDailySummaryQuery query,
        CancellationToken cancellationToken)
    {
        var date = query.Date ?? dateTimeProvider.UtcNow;
        var nextDate = date.AddDays(1);
        var storeId = tenant.Id;

        var startOfDay = date.Date;
        var endOfDay = nextDate.Date;

        var dailyOrdersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Where(o => o.CreatedAt >= startOfDay && o.CreatedAt < endOfDay);

        var ordersMetrics = await GetOrdersMetricsAsync(dailyOrdersQuery, cancellationToken);
        var revenueMetrics = await GetRevenueMetricsAsync(dailyOrdersQuery, cancellationToken);
        var topProduct = await GetTopProductAsync(dailyOrdersQuery, cancellationToken);
        var byChannel = await GetChannelMetricsAsync(dailyOrdersQuery, cancellationToken);
        var byPaymentStatus = await GetPaymentStatusMetricsAsync(dailyOrdersQuery, cancellationToken);

        return new DailySummaryResponse(
            Date: date,
            Orders: ordersMetrics,
            Revenue: revenueMetrics,
            TopProduct: topProduct,
            ByChannel: byChannel,
            ByPaymentStatus: byPaymentStatus
        );
    }

    private static async Task<OrdersMetrics> GetOrdersMetricsAsync(
        IQueryable<Order> query,
        CancellationToken cancellationToken)
    {
        var metrics = await query
            .GroupBy(_ => 1)
            .Select(g => new OrdersMetrics(
                Total: g.Count(),
                Confirmed: g.Count(o => o.PaymentStatus == PaymentStatus.Verified),
                Pending: g.Count(o => o.PaymentStatus == PaymentStatus.Pending),
                AwaitingVerification: g.Count(o => o.PaymentStatus == PaymentStatus.AwaitingVerification),
                Rejected: g.Count(o => o.PaymentStatus == PaymentStatus.Rejected),
                Canceled: g.Count(o => o.Status == OrderStatus.Canceled)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return metrics ?? new OrdersMetrics(0, 0, 0, 0, 0, 0);
    }

    private static async Task<RevenueMetrics> GetRevenueMetricsAsync(
        IQueryable<Order> query,
        CancellationToken cancellationToken)
    {
        var metrics = await query
            .Where(o => o.Status != OrderStatus.Canceled)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Confirmed = g
                    .Where(o => o.PaymentStatus == PaymentStatus.Verified)
                    .Sum(o => o.Total),
                Pending = g
                    .Where(o => o.PaymentStatus == PaymentStatus.Pending ||
                                o.PaymentStatus == PaymentStatus.AwaitingVerification)
                    .Sum(o => o.Total)
            })
            .FirstOrDefaultAsync(cancellationToken);

        decimal confirmed = metrics?.Confirmed ?? 0;
        decimal pending = metrics?.Pending ?? 0;

        return new RevenueMetrics(
            Confirmed: confirmed,
            Pending: pending,
            Total: confirmed + pending
        );
    }

    private async Task<TopProductItem?> GetTopProductAsync(
        IQueryable<Order> ordersQuery,
        CancellationToken cancellationToken)
    {
        var orderIds = await ordersQuery
            .Where(o => o.Status != OrderStatus.Canceled)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var topProduct = await db.OrderItems
            .AsNoTracking()
            .Where(oi => orderIds.Contains(oi.OrderId))
            .GroupBy(oi => new { oi.ProductVariant!.ProductId, oi.ProductName, oi.VariantId })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.VariantId,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(g => g.QuantitySold)
            .FirstOrDefaultAsync(cancellationToken);

        if (topProduct is null)
        {
            return null;
        }

        string? imageUrl = await db.ProductImages
            .AsNoTracking()
            .Where(i => i.VariantId == topProduct.VariantId)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefaultAsync(cancellationToken);

        return new TopProductItem(
            topProduct.ProductId,
            topProduct.ProductName,
            imageUrl,
            topProduct.QuantitySold,
            topProduct.Revenue
        );
    }

    private static async Task<IReadOnlyList<ChannelMetrics>> GetChannelMetricsAsync(
        IQueryable<Order> query,
        CancellationToken cancellationToken)
    {
        var channelData = await query
            .Where(o => o.Status != OrderStatus.Canceled)
            .GroupBy(o => o.Channel)
            .Select(g => new
            {
                Channel = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.Total)
            })
            .ToListAsync(cancellationToken);

        int totalOrders = channelData.Sum(c => c.OrderCount);

        return
        [
            .. channelData
                .Select(c => new ChannelMetrics(
                    Channel: c.Channel,
                    OrderCount: c.OrderCount,
                    Revenue: c.Revenue,
                    Percentage: totalOrders > 0
                        ? Math.Round((decimal)c.OrderCount / totalOrders * 100, 2)
                        : 0
                ))
                .OrderByDescending(c => c.OrderCount)
        ];
    }

    private static async Task<IReadOnlyList<PaymentStatusMetrics>> GetPaymentStatusMetricsAsync(
        IQueryable<Order> query,
        CancellationToken cancellationToken)
    {
        var statusData = await query
            .GroupBy(o => o.PaymentStatus)
            .Select(g => new
            {
                Status = g.Key,
                OrderCount = g.Count(),
                Revenue = g.Sum(o => o.Total)
            })
            .ToListAsync(cancellationToken);

        int totalOrders = statusData.Sum(s => s.OrderCount);

        return
        [
            .. statusData
                .Select(s => new PaymentStatusMetrics(
                    Status: s.Status,
                    OrderCount: s.OrderCount,
                    Revenue: s.Revenue,
                    Percentage: totalOrders > 0
                        ? Math.Round((decimal)s.OrderCount / totalOrders * 100, 2)
                        : 0
                ))
                .OrderByDescending(s => s.OrderCount)
        ];
    }
}
