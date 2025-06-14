using Domain.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Coupons;

internal sealed class CouponRuleConfiguration : IEntityTypeConfiguration<CouponRule>
{
    public void Configure(EntityTypeBuilder<CouponRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.CouponId)
            .IsRequired();

        builder.Property(r => r.RuleType)
            .IsRequired();

        builder.Property(r => r.Value)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();
    }
}
