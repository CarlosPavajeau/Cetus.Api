using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.PaymentLinks.Find;

internal sealed class FindPaymentLinkQueryHandler(
    IApplicationDbContext db,
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider
) : IQueryHandler<FindPaymentLinkQuery, PaymentLinkResponse>
{
    public async Task<Result<PaymentLinkResponse>> Handle(FindPaymentLinkQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var paymentLink = await db.PaymentLinks
            .Where(pl => pl.Token == query.Token)
            .FirstOrDefaultAsync(cancellationToken);

        if (paymentLink is null)
        {
            return Result.Failure<PaymentLinkResponse>(PaymentLinkErrors.NotFound(query.Token));
        }

        string baseUrl = configuration["App:PublicUrl"]!;
        string url = $"{baseUrl}/pay/{paymentLink.Token}";

        var timeRemaining = paymentLink.ExpiresAt > now
            ? paymentLink.ExpiresAt - now
            : TimeSpan.Zero;

        return new PaymentLinkResponse(
            paymentLink.Id,
            paymentLink.OrderId,
            paymentLink.Token,
            url,
            paymentLink.Status,
            paymentLink.ExpiresAt,
            paymentLink.CreatedAt,
            timeRemaining.Milliseconds
        );
    }
}
