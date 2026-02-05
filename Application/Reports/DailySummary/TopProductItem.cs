namespace Application.Reports.DailySummary;

public sealed record TopProductItem(
    Guid ProductId,
    string ProductName,
    string? ImageUrl,
    int QuantitySold,
    decimal Revenue
);
