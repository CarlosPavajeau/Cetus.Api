using System.ComponentModel.DataAnnotations;

namespace Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; set; }

    public DocumentType? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }

    public string Name { get; set; } = string.Empty;

    [EmailAddress] public string? Email { get; set; }

    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
