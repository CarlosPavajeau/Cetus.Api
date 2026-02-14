using Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Infrastructure.Email;

public class ResendEmailSender(IResend resend, IOptions<ResendSettings> options, ILogger<ResendEmailSender> logger)
    : IEmailSender
{
    private readonly ResendSettings _settings = options.Value;

    public async Task SendEmail(string subject, string body, string to, CancellationToken cancellationToken = default)
    {
        await SendEmail(subject, body, [to], cancellationToken);
    }

    public async Task SendEmail(string subject, string body, IReadOnlyCollection<string> to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string senderEmail = _settings.From;
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
