using System.Linq.Expressions;
using Domain.Orders;

namespace Application.Customers.Find;

public sealed record CustomerResponse(string Id, string Name, string Email, string Phone)
{
    public static Expression<Func<Customer, CustomerResponse>> Map => customer =>
        new CustomerResponse(customer.DocumentNumber ?? string.Empty, customer.Name, customer.Email, customer.Phone);
}
