using Application.Abstractions.Configurations;
using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using SharedKernel;

namespace Infrastructure.Reviews.Jobs;

internal sealed class SendPendingReviewRequestsJob(
    IApplicationDbContext db,
    IEmailSender emailSender,
    IDateTimeProvider dateTimeProvider,
    IOptions<AppSettings> appOptions,
    ILogger<SendPendingReviewRequestsJob> logger)
    : IJob
{
    public const string Name = nameof(SendPendingReviewRequestsJob);
    private const string EmailSubject = "¡Cuéntanos sobre tu experiencia!";

    private readonly AppSettings _appSettings = appOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Executing {JobName} at {ExecutionTime}", Name, context.FireTimeUtc);

        var today = dateTimeProvider.UtcNow;
        var todayStart = today.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var pendingRequests = await db.ReviewRequests
            .AsNoTracking()
            .Include(rr => rr.Customer)
            .Include(rr => rr.OrderItem)
            .Where(rr => rr.Status == ReviewRequestStatus.Pending
                         && rr.SendAt >= todayStart
                         && rr.SendAt < tomorrowStart
                         && rr.Customer!.Email != null)
            .ToListAsync(context.CancellationToken);

        if (pendingRequests.Count == 0)
        {
            logger.LogInformation("No pending review requests to send at {ExecutionTime}", context.FireTimeUtc);
            return;
        }

        string baseUrl = _appSettings.PublicUrl;
        int sentCount = 0;
        int failedCount = 0;

        await Task.WhenAll(pendingRequests.Select(async request =>
        {
            try
            {
                string reviewUrl = $"{baseUrl}/reviews/new?token={request.Token}";
                string messageBody = BuildMessageBody(request, reviewUrl);

                await emailSender.SendEmail(EmailSubject, messageBody, request.Customer!.Email!,
                    context.CancellationToken);

                await db.ReviewRequests
                    .Where(rr => rr.Id == request.Id)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(rr => rr.Status, ReviewRequestStatus.Sent),
                        context.CancellationToken);

                Interlocked.Increment(ref sentCount);
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref failedCount);
                logger.LogError(e, "Error sending review request email to {Customer}: {Error}",
                    request.Customer!.Name, e.Message);
            }
        }));

        logger.LogInformation("Completed processing review request emails: {SentCount} sent, {FailedCount} failed",
            sentCount, failedCount);
    }

    private string BuildMessageBody(ReviewRequest request, string reviewUrl)
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
                             background-color: #4169E1;
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
                         .product-details {
                             background-color: #f9f9f9;
                             padding: 15px;
                             border-radius: 5px;
                             margin: 20px 0;
                         }
                         .button {
                             display: inline-block;
                             background-color: #4169E1;
                             color: white;
                             padding: 12px 24px;
                             text-decoration: none;
                             border-radius: 5px;
                             font-weight: bold;
                             margin: 20px 0;
                         }
                         .footer {
                             text-align: center;
                             margin-top: 30px;
                             font-size: 12px;
                             color: #777;
                         }
                     </style>
                 </head>
                 <body>
                     <div class="header">
                         <h1>¡Cuéntanos sobre tu experiencia!</h1>
                     </div>

                     <div class="content">
                         <p>Hola <strong>{{request.Customer?.Name}}</strong>,</p>

                         <p>Esperamos que hayas disfrutado tu compra.</p>

                         <p>Tu opinión es muy importante para nosotros y nos ayudaría mucho si pudieras compartir tu experiencia con el producto que recibiste.</p>

                         <div class="product-details">
                             <h3>Producto:</h3>
                             <p><strong>{{request.OrderItem?.ProductName}}</strong></p>
                         </div>

                         <div style="text-align: center;">
                             <a href="{{reviewUrl}}" class="button">Escribir Reseña</a>
                         </div>

                         <p>Gracias por elegirnos. ¡Esperamos verte pronto!</p>
                     </div>

                     <div class="footer">
                         <p>Este es un correo automático, por favor no lo respondas directamente.</p>
                         <p>© {{dateTimeProvider.UtcNow.Year}} - TELEDIGITAL JYA</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
