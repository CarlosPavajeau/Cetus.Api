using System.Linq.Expressions;
using Domain.Orders;

namespace Application.Customers.Find;

public sealed record CustomerResponse(
    Guid Id,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string Name,
    string? Email,
    string Phone,
    DateTime? Since = null,
    int TotalPurchases = 0,
    decimal TotalSpent = 0,
    double? PurchaseFrequencyDays = null
)
{
    public static Expression<Func<Customer, CustomerResponse>> Map => customer =>
        new CustomerResponse(
            customer.Id,
            customer.DocumentType,
            customer.DocumentNumber,
            customer.Name,
            customer.Email,
            customer.Phone
        );
}
