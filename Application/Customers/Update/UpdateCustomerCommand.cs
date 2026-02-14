using Application.Abstractions.Messaging;
using Application.Customers.Find;
using Domain.Customers;

namespace Application.Customers.Update;

public sealed record UpdateCustomerCommand(
    Guid Id,
    DocumentType? DocumentType,
    string? DocumentNumber,
    string Name,
    string? Email,
    string Phone,
    string? Address
) : ICommand<CustomerResponse>;
