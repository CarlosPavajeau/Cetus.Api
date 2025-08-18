using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductOptionValueConfiguration : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductOptionValue> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Value)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(p => p.ProductOptionType)
            .WithMany(p => p.ProductOptionValues)
            .HasForeignKey(p => p.OptionTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
