using Application.Abstractions.Messaging;

namespace Application.Customers.Find;

public sealed record FindCustomerQuery(Guid Id) : IQuery<CustomerResponse>;
