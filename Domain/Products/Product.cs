using System.ComponentModel.DataAnnotations;
using Domain.Categories;

namespace Domain.Products;

public sealed class Product
{
    [Required] [Key] public Guid Id { get; set; }

    [Required] [MaxLength(256)] public string Name { get; set; } = string.Empty;

    [Required] [MaxLength(256)] public string Slug { get; set; } = string.Empty;

    [MaxLength(512)] public string? Description { get; set; }

    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Enabled { get; set; }

    public int SalesCount { get; set; }

    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }

    [MaxLength(512)] public string? ImageUrl { get; set; }

    [Required] public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
