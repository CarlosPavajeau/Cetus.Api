using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.HasKey(p => new {p.ProductId, p.OptionTypeId});

        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.ProductOptionType)
            .WithMany()
            .HasForeignKey(p => p.OptionTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
