using Application.Abstractions.Messaging;

namespace Application.Customers.Find;

public sealed record FindCustomerQuery(string Id) : IQuery<CustomerResponse>;
