using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.PaymentLinks.FindState;

internal sealed class FindPaymentLinkStateQueryHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<FindPaymentLinkStateQuery, PaymentLinkStateResponse>
{
    public async Task<Result<PaymentLinkStateResponse>> Handle(FindPaymentLinkStateQuery query,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId && o.StoreId == tenant.Id)
            .Select(o => new { o.Id, o.PaymentStatus, o.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<PaymentLinkStateResponse>(OrderErrors.NotFound(query.OrderId));
        }

        if (order.PaymentStatus == PaymentStatus.Verified)
        {
            return new PaymentLinkStateResponse(false, PaymentLinkReasons.OrderAlreadyPaid, null);
        }

        if (order.Status == OrderStatus.Canceled)
        {
            return new PaymentLinkStateResponse(false, PaymentLinkReasons.OrderCancelled, null);
        }

        var activeLink = await db.PaymentLinks
            .AsNoTracking()
            .Where(pl => pl.OrderId == query.OrderId)
            .Where(pl => pl.Status == PaymentLinkStatus.Active)
            .Where(pl => pl.ExpiresAt > dateTimeProvider.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeLink is null)
        {
            return new PaymentLinkStateResponse(true, null, null);
        }

        var timeRemaining = activeLink.ExpiresAt > dateTimeProvider.UtcNow
            ? activeLink.ExpiresAt - dateTimeProvider.UtcNow
            : TimeSpan.Zero;
        string baseUrl = configuration["App:PublicUrl"]!;
        string url = $"{baseUrl}/pay/{activeLink.Token}";

        return new PaymentLinkStateResponse(
            false,
            PaymentLinkReasons.ActiveLinkExists,
            new PaymentLinkResponse(
                activeLink.Id,
                activeLink.OrderId,
                activeLink.Token,
                url,
                activeLink.Status,
                activeLink.ExpiresAt,
                activeLink.CreatedAt,
                timeRemaining.Milliseconds
            )
        );
    }
}
