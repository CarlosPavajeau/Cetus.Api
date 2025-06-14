using System.ComponentModel.DataAnnotations;

namespace Domain.Orders;

public sealed class Customer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [EmailAddress] public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
