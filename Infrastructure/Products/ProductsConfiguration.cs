using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

internal sealed class ProductsConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Description)
            .HasMaxLength(512);

        builder.Property(p => p.Price)
            .IsRequired();

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(512);
    }
}
