using Application.Abstractions.Messaging;
using Application.Customers.Find;

namespace Application.Customers.FindByPhone;

public sealed record FindCustomerByPhoneQuery(string Phone) : IQuery<CustomerResponse>;
