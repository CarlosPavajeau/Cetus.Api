namespace Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendEmail(string subject, string body, string to, CancellationToken cancellationToken = default);

    Task SendEmail(string subject, string body, IReadOnlyCollection<string> to,
        CancellationToken cancellationToken = default);
}
