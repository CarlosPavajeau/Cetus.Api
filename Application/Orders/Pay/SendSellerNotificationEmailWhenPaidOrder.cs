using System.Collections.Immutable;
using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Pay;

internal sealed class SendSellerNotificationEmailWhenPaidOrder(
    IApplicationDbContext db,
    IEmailSender emailSender,
    ILogger<SendSellerNotificationEmailWhenPaidOrder> logger) : IDomainEventHandler<PaidOrderDomainEvent>
{
    private static readonly CultureInfo ColombianCulture = new("es-CO");

    public async Task Handle(PaidOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending email to sellers for order {OrderNumber}", domainEvent.Order.OrderNumber);

        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == domainEvent.Order.Id)
            .Select(o => new { o.Id, o.OrderNumber, o.StoreId })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order with ID {OrderId} not found", domainEvent.Order.Id);
            return;
        }

        var store = await db.Stores
            .Where(s => s.Id == order.StoreId)
            .Select(s => new { s.Name, s.ExternalId })
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store with ID {StoreId} not found", order.StoreId);
            return;
        }

        var storeMembers = await db.Users
            .FromSql($"""
                      SELECT u.id, u.name, u.email
                      FROM users u
                      INNER JOIN members m ON u.id = m.user_id
                      WHERE m.organization_id = {store.ExternalId} AND u.banned = FALSE;
                      """)
            .ToListAsync(cancellationToken);


        if (storeMembers.Count == 0)
        {
            logger.LogWarning("No store members found in the database");
            return;
        }

        var notificationEmails = storeMembers.Select(s => s.Email).ToImmutableList();
        string emailSubject = $"¡Pago recibido! Orden #{order.OrderNumber}";
        string messageBody = BuildMessageBody(domainEvent.Order.Customer, domainEvent.Order.Total, order.OrderNumber);

        await emailSender.SendEmail(emailSubject, messageBody, notificationEmails, cancellationToken);

        logger.LogInformation("Notification email sent for order {OrderId}", order.Id);
    }

    private static string BuildMessageBody(string customerName, decimal total, long orderNumber)
    {
        string formattedTotal = total.ToString("C", ColombianCulture);

        return $"""
                <html>
                <body>
                    <div>
                        <p>¡Hola!</p>
                        <p>Recibiste un pago por tu venta:</p>
                        <p>
                        Cliente: {customerName}<br>
                        Total: {formattedTotal}<br>
                        Número de orden: {orderNumber}<br>
                        </p>
                    </div>
                </body>
                </html>
                """;
    }
}
