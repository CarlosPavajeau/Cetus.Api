using System.ComponentModel.DataAnnotations;

namespace Domain.Categories;

public sealed class Category
{
    [Required] [Key] public Guid Id { get; set; }

    [MaxLength(256)] public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
