using Application.Abstractions.Messaging;
using Application.Orders;
using SharedKernel;

namespace Application.Customers.SearchAllOrders;

public sealed record SearchAllCustomerOrdersQuery(
    Guid CustomerId,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<SimpleOrderResponse>>;
