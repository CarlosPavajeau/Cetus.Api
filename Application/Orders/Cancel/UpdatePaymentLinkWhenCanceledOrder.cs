using Application.Abstractions.Data;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Orders.Cancel;

internal sealed class UpdatePaymentLinkWhenCanceledOrder(IApplicationDbContext db)
    : IDomainEventHandler<CanceledOrderDomainEvent>
{
    public async Task Handle(CanceledOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var orderId = domainEvent.OrderId;

        await db.PaymentLinks
            .Where(pl => pl.OrderId == orderId)
            .Where(pl => pl.Status == PaymentLinkStatus.Active)
            .ExecuteUpdateAsync(s =>
                    s.SetProperty(pl => pl.Status, PaymentLinkStatus.Expired),
                cancellationToken
            );
    }
}
