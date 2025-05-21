using System.ComponentModel.DataAnnotations;
using Domain.Products;

namespace Domain.Orders;

public sealed class OrderItem
{
    [Required] [Key] public Guid Id { get; set; }

    [Required] [MaxLength(256)] public string ProductName { get; set; } = string.Empty;

    [MaxLength(512)] public string? ImageUrl { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }

    [Required] public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
