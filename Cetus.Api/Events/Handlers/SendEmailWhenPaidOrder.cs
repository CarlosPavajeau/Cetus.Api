using System.Globalization;
using Cetus.Domain.Events;
using MediatR;
using Resend;

namespace Cetus.Api.Events.Handlers;

public sealed class SendEmailWhenPaidOrder : INotificationHandler<PaidOrderEvent>
{
    private static readonly CultureInfo ColombianCulture = new("es-CO");
    private const string EmailSubject = "¡Hemos recibido tu pago!";

    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendEmailWhenPaidOrder> _logger;

    public SendEmailWhenPaidOrder(ILogger<SendEmailWhenPaidOrder> logger, IResend resend, IConfiguration configuration)
    {
        _logger = logger;
        _resend = resend;
        _configuration = configuration;
    }

    public async Task Handle(PaidOrderEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending email to {Customer} for order {OrderNumber} with total {Total}",
            notification.Order.Customer, notification.Order.OrderNumber, notification.Order.Total);

        var messageBody = BuildEmailBody(notification);

        await SendNotificationEmail(
            email: notification.CustomerEmail,
            subject: EmailSubject,
            body: messageBody,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Email sent to {Customer} for order {OrderNumber} with total {Total}",
            notification.Order.Customer, notification.Order.OrderNumber, notification.Order.Total);
    }

    private async Task SendNotificationEmail(string email, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Cannot send email: recipient email address is null or empty");
            return;
        }

        try
        {
            var senderEmail = _configuration["Resend:From"]
                              ?? throw new InvalidOperationException(
                                  "Sender email configuration 'Resend:From' is missing");

            var message = new EmailMessage
            {
                From = senderEmail
            };

            message.To.Add(email);
            message.Subject = subject;
            message.HtmlBody = body;

            await _resend.EmailSendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}: {Error}", email, ex.Message);
            // We're deliberately not rethrowing to prevent the exception from bubbling up
            // A more sophisticated implementation might use a retry mechanism or queue
        }
    }

    private static string BuildEmailBody(PaidOrderEvent notification)
    {
        var formattedTotal = notification.Order.Total.ToString("C", ColombianCulture);

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
                         .order-details {
                             background-color: #f9f9f9;
                             padding: 15px;
                             border-radius: 5px;
                             margin: 20px 0;
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
                     </style>
                 </head>
                 <body>
                     <div class="header">
                         <h1>¡Gracias por tu compra!</h1>
                     </div>
                     
                     <div class="content">
                         <p>Hola <strong>{{notification.Order.Customer}}</strong>,</p>
                         
                         <p>¡Excelentes noticias! Hemos recibido tu pago correctamente. Estamos procesando tu pedido y te notificaremos cuando sea enviado.</p>
                         
                         <div class="order-details">
                             <h3>Detalles de tu pedido:</h3>
                             <p><strong>Número de pedido:</strong> #{{notification.Order.OrderNumber}}</p>
                             <p><strong>Monto total pagado:</strong> {{formattedTotal}}</p>
                             <p><strong>Fecha:</strong> {{DateTime.Now:dd/MM/yyyy}}</p>
                         </div>
                         
                         <p>¡Gracias por confiar en nosotros!</p>
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
