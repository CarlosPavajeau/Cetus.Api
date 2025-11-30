using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Products;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VariantId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.StockAfter)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(255);

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(100);

        builder.Property(x => x.UserId)
            .HasMaxLength(100);

        builder.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
