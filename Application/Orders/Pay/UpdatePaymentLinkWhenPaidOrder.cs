using Application.Abstractions.Data;
using Domain.Orders;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Pay;

internal sealed class UpdatePaymentLinkWhenPaidOrder(IApplicationDbContext db)
    : IDomainEventHandler<PaidOrderDomainEvent>
{
    public async Task Handle(PaidOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var orderId = domainEvent.Order.Id;

        await db.PaymentLinks
            .Where(pl => pl.OrderId == orderId)
            .Where(pl => pl.Status == PaymentLinkStatus.Active)
            .ExecuteUpdateAsync(s =>
                    s.SetProperty(pl => pl.Status, PaymentLinkStatus.Paid),
                cancellationToken
            );
    }
}
