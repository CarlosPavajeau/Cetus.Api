namespace Application.Reports.ProductProfitabilityRanking;

public sealed record ProductProfitabilityItem(
    Guid ProductId,
    string Product,
    Guid CategoryId,
    string Category,
    int UnitsSold,
    decimal Revenue,
    decimal Costs,
    decimal Profit,
    decimal MarginPercentage,
    bool IsStarProduct,
    bool IsProblematic
);
