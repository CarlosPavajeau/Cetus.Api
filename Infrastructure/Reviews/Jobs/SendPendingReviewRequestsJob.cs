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

public class SendPendingReviewRequestsJob(
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

        var pendingRequests = await db.ReviewRequests
            .Include(rr => rr.Customer)
            .Include(rr => rr.OrderItem)
            .Where(rr => rr.Status == ReviewRequestStatus.Pending && rr.SendAt.Date == today.Date)
            .ToListAsync();

        if (pendingRequests.Count == 0)
        {
            logger.LogInformation("No pending review requests to send at {ExecutionTime}", context.FireTimeUtc);
            return;
        }

        string baseUrl = _appSettings.PublicUrl;
        foreach (var request in pendingRequests)
        {
            if (request.Customer?.Email is null)
            {
                continue;
            }

            try
            {
                string reviewUrl = BuildReviewUrl(request, baseUrl);
                string messageBody = BuildMessageBody(request, reviewUrl);

                await emailSender.SendEmail(EmailSubject, messageBody, request.Customer.Email,
                    context.CancellationToken);

                request.Status = ReviewRequestStatus.Sent;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error sending review request email to {Customer}: {Error}",
                    request.Customer.Name, e.Message);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Completed processing {Count} review request emails", pendingRequests.Count);
    }

    private static string BuildReviewUrl(ReviewRequest request, string baseUrl)
    {
        return $"{baseUrl}/reviews/new?token={request.Token}";
    }

    private static string BuildMessageBody(ReviewRequest request, string reviewUrl)
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
                         <p>© {{DateTime.Now.Year}} - TELEDIGITAL JYA</p>
                     </div>
                 </body>
                 </html>
                 """;
    }
}
