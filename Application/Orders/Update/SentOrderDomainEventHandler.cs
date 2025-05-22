using Domain.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using SharedKernel;

namespace Application.Orders.Update;

internal sealed class SentOrderDomainEventHandler(
    IResend resend,
    IConfiguration configuration,
    ILogger<SentOrderDomainEventHandler> logger
) : IDomainEventHandler<SentOrderDomainEvent>
{
    private const string EmailSubject = "Tu pedido ha sido enviado!";

    public async Task Handle(SentOrderDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending email to {Customer} for order {OrderNumber}", domainEvent.Order.Customer,
            domainEvent.Order.OrderNumber);

        var messageBody = BuildMessageBody(domainEvent.Order);

        await SendNotificationEmail(domainEvent.Order.CustomerEmail, EmailSubject, messageBody, cancellationToken);

        logger.LogInformation("Email sent to {Customer} for order {OrderNumber}", domainEvent.Order.Customer,
            domainEvent.Order.OrderNumber);
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
            // We're deliberately not rethrowing to prevent the exception from bubbling up
            // A more sophisticated implementation might use a retry mechanism or queue
        }
    }

    private static string BuildMessageBody(SentOrder order)
    {
        return $$"""
                 <html>
                 <head>
                     <style>
                         body {
                             font-family: 'Arial', sans-serif;
                             max-width: 600px;
                             margin: 0 auto;
                             padding: 20px;
                             color: #333;
                             line-height: 1.6;
                         }
                         .header {
                             background-color: #4169E1; /* Royal Blue */
                             color: white;
                             padding: 15px;
                             text-align: center;
                             border-radius: 5px 5px 0 0;
                         }
                         .content {
                             padding: 20px;
                             border: 1px solid #ddd;
                             border-top: none;
                             border-radius: 0 0 5px 5px;
                         }
                         .shipping-details {
                             background-color: #f9f9f9;
                             padding: 15px;
                             border-radius: 5px;
                             margin: 20px 0;
                         }
                         .product-list {
                             width: 100%;
                             border-collapse: collapse;
                             margin: 20px 0;
                         }
                         .product-list th, .product-list td {
                             padding: 12px;
                             text-align: left;
                             border-bottom: 1px solid #ddd;
                         }
                         .product-list th {
                             background-color: #f2f2f2;
                         }
                         .footer {
                             text-align: center;
                             margin-top: 30px;
                             font-size: 12px;
                             color: #777;
                         }
                         .button {
                             display: inline-block;
                             background-color: #4169E1; /* Royal Blue */
                             color: white;
                             padding: 10px 20px;
                             text-decoration: none;
                             border-radius: 5px;
                             font-weight: bold;
                         }
                         .shipping-icon {
                             text-align: center;
                             font-size: 48px;
                             margin: 20px 0;
                         }
                     </style>
                 </head>
                 <body>
                     <div class="header">
                         <h1>¡Tu pedido ha sido enviado!</h1>
                     </div>
                     
                     <div class="content">
                         <p>Hola <strong>{{order.Customer}}</strong>,</p>
                         
                         <p>¡Buenas noticias! Tu pedido #{{order.OrderNumber}} ha sido completado y está en camino.</p>
                         
                         <div class="shipping-icon">
                         </div>
                         
                         <div class="shipping-details">
                             <h3>Información de envío:</h3>
                             <p><strong>Fecha de envío:</strong> {{DateTime.Now:dd/MM/yyyy}}</p>
                             <p><strong>Dirección de entrega:</strong> {{order.Address}}</p>
                         </div>
                         
                         <p>¡Esperamos que disfrutes tu compra!</p>
                     </div>
                     
                     <div class="footer">
                         <p>Este es un correo automático, por favor no lo respondas directamente.</p>
                         <p>© {{DateTime.Now.Year}} - TELEDIGITAL JYA</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
