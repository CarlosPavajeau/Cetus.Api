using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(oi => oi.ImageUrl)
            .HasMaxLength(512);

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        builder.Property(oi => oi.Price)
            .IsRequired();

        builder.Property(oi => oi.VariantId)
            .IsRequired();

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(oi => oi.VariantId);

        builder.Ignore(oi => oi.Product);
        builder.Ignore(oi => oi.ProductId);

        // Ensure ProductVariant is available for projections that derive ProductId
        builder.Navigation(oi => oi.ProductVariant).AutoInclude();
    }
}
