using Application.Abstractions.Data;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Infrastructure.PaymentLinks.Jobs;

public sealed class ExpirePaymentLinksJob(
    IApplicationDbContext db,
    IDateTimeProvider dateTimeProvider,
    ILogger<ExpirePaymentLinksJob> logger) : IJob
{
    public const string Name = nameof(ExpirePaymentLinksJob);

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting expire payment links job");

        var cancellationToken = context.CancellationToken;
        var now = dateTimeProvider.UtcNow;

        await db.PaymentLinks
            .Where(pl => pl.Status == PaymentLinkStatus.Active)
            .Where(pl => pl.ExpiresAt <= now)
            .ExecuteUpdateAsync(
                pl => pl.SetProperty(p => p.Status, PaymentLinkStatus.Expired),
                cancellationToken
            );

        logger.LogInformation("Expire payment links job completed");
    }
}
