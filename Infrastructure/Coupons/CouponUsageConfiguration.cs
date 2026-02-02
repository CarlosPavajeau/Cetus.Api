using Domain.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Coupons;

internal sealed class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.CouponId)
            .IsRequired();

        builder.Property(u => u.CustomerId)
            .IsRequired();

        builder.Property(u => u.OrderId)
            .IsRequired();

        builder.Property(u => u.DiscountAmountApplied)
            .IsRequired()
            .HasPrecision(12, 2);

        builder.Property(u => u.UsedAt)
            .IsRequired();
    }
}
