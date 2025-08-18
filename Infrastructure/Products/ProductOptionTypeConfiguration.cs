using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductOptionTypeConfiguration : IEntityTypeConfiguration<ProductOptionType>
{
    public void Configure(EntityTypeBuilder<ProductOptionType> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}
