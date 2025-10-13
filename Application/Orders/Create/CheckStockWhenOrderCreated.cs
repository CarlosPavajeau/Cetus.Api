using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class CheckStockWhenOrderCreated(
    IApplicationDbContext db,
    IEmailSender emailSender,
    ILogger<CheckStockWhenOrderCreated> logger
) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private const int LowStockThreshold = 5;
    private const string EmailSubject = "Alerta de stock bajo";

    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking stock after order {OrderId} created", domainEvent.Id);

        var purchasedVariants = await db.OrderItems
            .AsNoTracking()
            .Where(o => o.OrderId == domainEvent.Id)
            .Select(o => o.VariantId)
            .ToListAsync(cancellationToken);

        var lowStockVariants = await db.ProductVariants
            .AsNoTracking()
            .Where(v => purchasedVariants.Contains(v.Id))
            .Where(v => v.Stock <= LowStockThreshold)
            .Select(v => new {v.Id, v.Stock, v.Product!.Name})
            .ToListAsync(cancellationToken);

        if (lowStockVariants.Count == 0)
        {
            return;
        }

        var store = await db.Stores
            .AsNoTracking()
            .Where(s => s.Id == domainEvent.StoreId)
            .Select(s => new {s.Name, s.Email, s.ExternalId})
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store with ID {StoreId} not found in the database", domainEvent.StoreId);
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

        var messageBody = BuildMessageBody(store.Name, lowStockVariants.Select(v => (v.Name, v.Stock)));
        var notificationEmails = storeMembers.Select(m => m.Email).ToList();

        await emailSender.SendEmail(EmailSubject, messageBody, notificationEmails, cancellationToken);

        logger.LogInformation("Notification email sent to admin");
    }

    private static string BuildMessageBody(string storeName,
        IEnumerable<(string ProductName, int Stock)> lowStockVariants)
    {
        var variantsList = string.Join("",
            lowStockVariants.Select(v => $"<li>{v.ProductName}: {v.Stock} unidades restantes</li>"));

        return $$"""
                 <html>
                 <head>
                     <style>
                         body {
                             font-family: Arial, sans-serif;
                             background-color: #f4f4f4;
                             margin: 0;
                             padding: 20px;
                         }
                         .container {
                             background-color: #ffffff;
                             padding: 20px;
                             border-radius: 5px;
                             box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                         }
                         h2 {
                             color: #333333;
                         }
                         p {
                             color: #555555;
                         }
                         ul {
                             list-style-type: none;
                             padding: 0;
                         }
                         li {
                             background-color: #f9f9f9;
                             margin-bottom: 10px;
                             padding: 10px;
                             border-radius: 3px;
                             border: 1px solid #dddddd;
                         }
                     </style>
                 </head>
                 <body>
                     <div class="container">
                         <h2>Alerta de stock bajo para {{storeName}}</h2>
                         <p>Los siguientes productos tienen un stock bajo:</p>
                         <ul>
                             {{variantsList}}
                         </ul>
                         <p>Por favor, considere reabastecer estos productos pronto.</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
