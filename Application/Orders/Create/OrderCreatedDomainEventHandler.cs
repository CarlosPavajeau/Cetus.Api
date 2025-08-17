using Application.Abstractions.Data;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class OrderCreatedDomainEventHandler(
    IApplicationDbContext db,
    IResend resend,
    IConfiguration configuration,
    ILogger<OrderCreatedDomainEventHandler> logger
) : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private const string EmailSubject = "Nuevo pedido creado!";

    public async Task Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending notification email to admin for order {OrderId}", domainEvent.Id);

        var order = await db.Orders
            .Where(o => o.Id == domainEvent.Id)
            .Select(o => new {o.Id, o.OrderNumber, o.StoreId})
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order with ID {OrderId} not found in the database", domainEvent.Id);
            return;
        }

        var store = await db.Stores
            .Where(s => s.Id == order.StoreId)
            .Select(s => new {s.Name, s.Email, s.ExternalId})
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            logger.LogWarning("Store with ID {StoreId} not found in the database", order.StoreId);
            return;
        }

        if (store.Email is null)
        {
            logger.LogWarning("Store with ID {StoreId} does not have an email configured", order.StoreId);
            return;
        }

        var messageBody = BuildMessageBody(domainEvent);
        var notificationEmail = store.Email;

        await SendNotificationEmail(notificationEmail, EmailSubject, messageBody, cancellationToken);

        logger.LogInformation("Notification email sent to admin for order {OrderId}", domainEvent.Id);
    }

    private async Task SendNotificationEmail(string email, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var senderEmail = configuration["Resend:From"]
                              ?? throw new InvalidOperationException(
                                  "Sender email configuration 'Resend:From' is missing");

            var message = new EmailMessage
            {
                From = senderEmail
            };

            message.To.Add(email);
            message.Subject = subject;
            message.HtmlBody = body;

            await resend.EmailSendAsync(message, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending email to {Email}: {Error}", email, e.Message);
        }
    }

    private static string BuildMessageBody(OrderCreatedDomainEvent notification)
    {
        return $$"""
                 <html>
                 <head>
                     <style>
                         body {
                             font-family: Arial, sans-serif;
                             background-color: #f4f4f4;
                             padding: 20px;
                         }
                         .container {
                             background-color: #fff;
                             padding: 20px;
                             border-radius: 5px;
                             box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                         }
                         h1 {
                             color: #333;
                         }
                     </style>
                 </head>
                 <body>
                     <div class="container">
                         <h1>Nuevo pedido creado!</h1>
                         <p>Se ha creado un nuevo pedido con el número de orden {{notification.Id}}.</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
