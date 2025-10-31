namespace Application.Abstractions.Services;

public interface IStockReservationService
{
    Task<StockReservationResult> TryReserveAsync(
        IReadOnlyDictionary<long, int> quantitiesByVariant,
        Guid storeId,
        CancellationToken cancellationToken);
}

public sealed record StockReservationResult(
    bool Success,
    IReadOnlyList<long> ReservedVariantIds,
    IReadOnlyList<long> FailedVariantIds);


