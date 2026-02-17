namespace Application.Reports.MonthlyProfitability;

public sealed record ProductCostWarning(
    Guid ProductId,
    string ProductName,
    long VariantId,
    string Sku
);
