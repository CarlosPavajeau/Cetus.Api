using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SharedKernel;

namespace Application.PaymentLinks.Create;

internal sealed class CreatePaymentLinkCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreatePaymentLinkCommand, PaymentLinkResponse>
{
    public async Task<Result<PaymentLinkResponse>> Handle(CreatePaymentLinkCommand command,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == command.OrderId && o.StoreId == tenant.Id)
            .Select(o => new { o.Id, o.PaymentStatus, o.Status, o.Total })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<PaymentLinkResponse>(OrderErrors.NotFound(command.OrderId));
        }

        if (order.Status == OrderStatus.Canceled)
        {
            return Result.Failure<PaymentLinkResponse>(OrderErrors.AlreadyCanceled(command.OrderId));
        }

        if (order.PaymentStatus == PaymentStatus.Verified)
        {
            return Result.Failure<PaymentLinkResponse>(PaymentLinkErrors.AlreadyPaid(command.OrderId));
        }

        // Fast-fail: check for an active link before starting the transaction
        bool existingLink = await db.PaymentLinks
            .AsNoTracking()
            .Where(pl => pl.OrderId == command.OrderId)
            .Where(pl => pl.Status == PaymentLinkStatus.Active)
            .Where(pl => pl.ExpiresAt > dateTimeProvider.UtcNow)
            .AnyAsync(cancellationToken);

        if (existingLink)
        {
            return Result.Failure<PaymentLinkResponse>(PaymentLinkErrors.ActiveLinkExists(command.OrderId));
        }

        string token = GenerateSecureToken();
        int expirationHours = command.ExpirationHours > 0 ? command.ExpirationHours : 24;
        var now = dateTimeProvider.UtcNow;

        var paymentLink = new PaymentLink
        {
            Id = Guid.CreateVersion7(),
            OrderId = command.OrderId,
            Token = token,
            Status = PaymentLinkStatus.Active,
            ExpiresAt = now.AddHours(expirationHours),
            CreatedAt = now
        };

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        try
        {
            // Mark previous links as expired
            await db.PaymentLinks
                .Where(pl => pl.OrderId == command.OrderId)
                .Where(pl => pl.Status == PaymentLinkStatus.Active)
                .ExecuteUpdateAsync(s =>
                        s.SetProperty(pl => pl.Status, PaymentLinkStatus.Expired),
                    cancellationToken
                );

            await db.PaymentLinks.AddAsync(paymentLink, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation
                                           })
        {
            return Result.Failure<PaymentLinkResponse>(PaymentLinkErrors.ActiveLinkExists(command.OrderId));
        }

        string baseUrl = configuration["App:PublicUrl"]!;
        string url = $"{baseUrl}/pay/{token}";

        return new PaymentLinkResponse(
            paymentLink.Id,
            paymentLink.OrderId,
            paymentLink.Token,
            url,
            paymentLink.Status,
            paymentLink.ExpiresAt,
            paymentLink.CreatedAt,
            paymentLink.ExpiresAt - now
        );
    }

    private static string GenerateSecureToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
