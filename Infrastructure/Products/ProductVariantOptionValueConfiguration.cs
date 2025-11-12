using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductVariantOptionValueConfiguration : IEntityTypeConfiguration<ProductVariantOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantOptionValue> builder)
    {
        builder.HasKey(p => new {p.VariantId, p.OptionValueId});

        builder.HasIndex(p => p.OptionValueId);

        builder.HasOne(p => p.ProductOptionValue)
            .WithMany()
            .HasForeignKey(p => p.OptionValueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ProductVariant)
            .WithMany(p => p.OptionValues)
            .HasForeignKey(p => p.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
