namespace Application.Customers.SearchAll;

public sealed record CustomerSummaryResponse(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    int TotalOrders,
    decimal TotalSpent,
    DateTime? LastPurchase
);
