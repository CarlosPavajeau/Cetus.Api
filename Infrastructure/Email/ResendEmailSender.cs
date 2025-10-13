using Application.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Infrastructure.Email;

public class ResendEmailSender(IResend resend, IConfiguration configuration, ILogger<ResendEmailSender> logger)
    : IEmailSender
{
    public async Task SendEmail(string subject, string body, string to, CancellationToken cancellationToken = default)
    {
        await SendEmail(subject, body, [to], cancellationToken);
    }

    public async Task SendEmail(string subject, string body, IReadOnlyCollection<string> to,
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

            var emailAddress = to.Select(e => new EmailAddress
            {
                Email = e
            });

            message.To.AddRange(emailAddress);

            message.Subject = subject;
            message.HtmlBody = body;

            await resend.EmailSendAsync(message, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending email to {To}", string.Join(", ", to));
        }
    }
}
