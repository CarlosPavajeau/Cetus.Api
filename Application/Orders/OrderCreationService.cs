using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SharedKernel;

namespace Application.Orders;

internal sealed record VariantInfo(long Id, decimal Price, string ProductName, string? ImageUrl = null);

internal sealed class OrderCreationService(
    IApplicationDbContext context,
    ITenantContext tenant,
    IStockReservationService stockReservationService
)
{
    public async Task<Result<List<VariantInfo>>> ValidateAndGetVariantsAsync(
        IReadOnlyList<long> variantIds,
        Dictionary<long, int> quantitiesByVariant,
        int itemCount,
        CancellationToken cancellationToken)
    {
        if (itemCount == 0)
        {
            return Result.Failure<List<VariantInfo>>(OrderErrors.EmptyOrder());
        }

        if (quantitiesByVariant.Values.Any(q => q <= 0))
        {
            return Result.Failure<List<VariantInfo>>(OrderErrors.InvalidItemQuantities());
        }

        var variants = await context.ProductVariants
            .AsNoTracking()
            .Where(v =>
                variantIds.Contains(v.Id) &&
                v.DeletedAt == null &&
                v.Product != null &&
                v.Product.DeletedAt == null &&
                v.Product.StoreId == tenant.Id)
            .Select(v => new VariantInfo(
                v.Id,
                v.Price,
                v.Product!.Name,
                v.Images.Select(img => img.ImageUrl).FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);

        var foundVariantIds = variants.Select(p => p.Id).ToHashSet();
        var missingProducts = variantIds.Except(foundVariantIds).ToList();

        if (missingProducts.Count != 0)
        {
            var productCodes = missingProducts.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList();
            return Result.Failure<List<VariantInfo>>(OrderErrors.ProductsNotFound(productCodes));
        }

        return variants;
    }

    public async Task<Result> ReserveStockOrFailAsync(
        Dictionary<long, int> quantitiesByVariant,
        Guid orderId,
        List<VariantInfo> variants,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var reserveResult = await stockReservationService.TryReserveAsync(
            quantitiesByVariant, orderId, tenant.Id, cancellationToken);

        if (!reserveResult.Success)
        {
            await transaction.RollbackAsync(cancellationToken);

            var variantsById = variants.ToDictionary(v => v.Id);
            var outOfStockProducts = reserveResult.FailedVariantIds
                .Select(id =>
                    variantsById.TryGetValue(id, out var variant)
                        ? variant.ProductName
                        : id.ToString(CultureInfo.InvariantCulture))
                .ToList();

            var requestedProducts = reserveResult.FailedVariantIds
                .Select(id =>
                {
                    string label = variantsById.TryGetValue(id, out var variant)
                        ? variant.ProductName
                        : id.ToString(CultureInfo.InvariantCulture);
                    string quantity = quantitiesByVariant.TryGetValue(id, out int qty) ? $"{qty}" : "unknown";

                    return $"{label} (requested: {quantity})";
                })
                .ToList();

            return Result.Failure(OrderErrors.InsufficientStock(outOfStockProducts, requestedProducts));
        }

        return Result.Success();
    }
}
