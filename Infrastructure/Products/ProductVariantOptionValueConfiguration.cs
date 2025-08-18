using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductVariantOptionValueConfiguration : IEntityTypeConfiguration<ProductVariantOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantOptionValue> builder)
    {
        builder.HasKey(p => new {p.VariantId, p.OptionValueId});
    }
}
