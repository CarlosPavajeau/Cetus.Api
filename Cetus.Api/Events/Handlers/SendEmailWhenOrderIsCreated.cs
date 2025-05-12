using Cetus.Orders.Domain.Events;
using MediatR;
using Resend;

namespace Cetus.Api.Events.Handlers;

public sealed class SendEmailWhenOrderIsCreated : INotificationHandler<OrderCreatedEvent>
{
    private const string EmailSubject = "Nuevo pedido creado!";

    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendEmailWhenOrderIsCreated> _logger;

    public SendEmailWhenOrderIsCreated(IResend resend, IConfiguration configuration,
        ILogger<SendEmailWhenOrderIsCreated> logger)
    {
        _resend = resend;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending notification email to admin for order {OrderNumber}",
            notification.Order.OrderNumber);

        var messageBody = BuildMessageBody(notification);
        var notificationEmail = _configuration["Notification:Email"];
        if (string.IsNullOrEmpty(notificationEmail))
        {
            throw new InvalidOperationException("Notification email configuration 'Notification:Email' is missing");
        }

        await SendNotificationEmail(notificationEmail, EmailSubject, messageBody, cancellationToken);

        _logger.LogInformation("Notification email sent to admin for order {OrderNumber}",
            notification.Order.OrderNumber);
    }

    private async Task SendNotificationEmail(string email, string subject, string body,
        CancellationToken cancellationToken = default)
    {
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending email to {Email}: {Error}", email, e.Message);
        }
    }

    private static string BuildMessageBody(OrderCreatedEvent notification)
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
                         <p>Se ha creado un nuevo pedido con el número de orden {{notification.Order.OrderNumber}}.</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
