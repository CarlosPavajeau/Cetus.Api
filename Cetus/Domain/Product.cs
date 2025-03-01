using System.ComponentModel.DataAnnotations;

namespace Cetus.Domain;

public class Product
{
    [Required] [Key] public Guid Id { get; set; }

    [Required] [MaxLength(256)] public string Name { get; set; } = string.Empty;

    [MaxLength(512)] public string? Description { get; set; }

    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Enabled { get; set; }

    [MaxLength(512)] public string? ImageUrl { get; set; }

    [Required] public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
