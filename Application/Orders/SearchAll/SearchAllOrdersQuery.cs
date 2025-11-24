using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Orders.SearchAll;

public sealed record SearchAllOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    string[]? Statuses = null,
    DateTime? From = null,
    DateTime? To = null
) : IQuery<PagedResult<OrderResponse>>;
