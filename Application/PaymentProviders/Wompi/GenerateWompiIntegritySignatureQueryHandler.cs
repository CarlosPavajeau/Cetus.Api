using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.PaymentProviders.Wompi;

internal sealed class GenerateWompiIntegritySignatureQueryHandler(IApplicationDbContext db, ITenantContext tenant)
    : IQueryHandler<GenerateWompiIntegritySignatureQuery, string>
{
    public async Task<Result<string>> Handle(GenerateWompiIntegritySignatureQuery query,
        CancellationToken cancellationToken)
    {
        string? integritySecret = await db.Stores
            .AsNoTracking()
            .Where(s => s.Id == tenant.Id)
            .Select(s => s.WompiIntegrityKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(integritySecret))
        {
            return Result.Failure<string>(Error.Problem("Store.NotIntegrityKey", "Llave de ingegridad no configurada"));
        }

        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == query.OrderId)
            .Select(o => new { o.Id, o.OrderNumber, o.Total })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<string>(OrderErrors.NotFound(query.OrderId));
        }

        string integritySignature = GenerateIntegritySignature(order.Id, order.Total, integritySecret);

        return integritySignature;
    }

    private static string GenerateIntegritySignature(Guid orderId, decimal total, string integritySecret)
    {
        long amountInCents = (long)(total * 100);
        string concatenated = $"{orderId}{amountInCents}COP{integritySecret}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(concatenated));
        return Convert.ToHexStringLower(hash);
    }
}
