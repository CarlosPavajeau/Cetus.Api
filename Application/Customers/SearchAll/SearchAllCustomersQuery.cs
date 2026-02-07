using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Customers.SearchAll;

public enum CustomerSortBy
{
    Name,
    TotalSpent,
    LastPurchase
}

public sealed record SearchAllCustomersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null
) : IQuery<PagedResult<CustomerSummaryResponse>>;
