using Domain.Categories;
using NpgsqlTypes;

namespace Domain.Products;

public sealed class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Enabled { get; set; }

    public int SalesCount { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<ProductVariant> Variants { get; set; } = new HashSet<ProductVariant>();

    public Guid StoreId { get; set; }

    public NpgsqlTsVector? SearchVector { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}
