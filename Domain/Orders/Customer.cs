using System.ComponentModel.DataAnnotations;

namespace Domain.Orders;

public sealed class Customer
{
    [Required] [MaxLength(50)] [Key] public string Id { get; set; } = string.Empty;

    [Required] [MaxLength(256)] public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required] [MaxLength(256)] public string Phone { get; set; } = string.Empty;

    [Required] [MaxLength(256)] public string Address { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
