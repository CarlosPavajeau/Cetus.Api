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

        await SendNotificationEmail(
            notification.CustomerEmail,
            "Hemos recibido tu pago!",
            $"Recibimos tu pago por un total de ${notification.Order.Total} por la orden {notification.Order.OrderNumber}. Gracias por tu compra!");

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
}
