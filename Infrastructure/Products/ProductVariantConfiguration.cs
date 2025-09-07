using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(100)
            .IsRequired();
        
        
        builder.HasIndex(p => new {p.ProductId, p.Sku})
            .IsUnique();

        builder.HasOne(p => p.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(p => p.Images)
            .WithOne(i => i.ProductVariant)
            .HasForeignKey(i => i.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
