using Cetus.Domain.Events;
using MediatR;
using Resend;

namespace Cetus.Api.Events.Handlers;

public sealed class SendEmailWhenPaidOrder : INotificationHandler<PaidOrderEvent>
{
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

        var messageBody = BuildMessageBody(notification);

        await SendNotificationEmail(notification.CustomerEmail, "Hemos recibido tu pago!", messageBody);

        _logger.LogInformation("Email sent to {Customer} for order {OrderNumber} with total {Total}",
            notification.Order.Customer, notification.Order.OrderNumber, notification.Order.Total);
    }

    private async Task SendNotificationEmail(string email, string subject, string body)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _configuration["Resend:From"]!
            };

            message.To.Add(email);
            message.Subject = subject;
            message.TextBody = body;

            await _resend.EmailSendAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending email to {Email}: {Error}", email, e.Message);
        }
    }

    private static string BuildMessageBody(PaidOrderEvent notification)
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
                             <p><strong>Monto total pagado:</strong> ${{notification.Order.Total:C}}</p>
                             <p><strong>Fecha:</strong> {{DateTime.Now:dd/MM/yyyy}}</p>
                         </div>
                         
                         
                         <p>¡Gracias por confiar en nosotros!</p>
                     </div>
                     
                     <div class="footer">
                         <p>Este es un correo automático, por favor no lo respondas directamente.</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
