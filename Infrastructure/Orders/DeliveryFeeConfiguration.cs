using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

public class DeliveryFeeConfiguration : IEntityTypeConfiguration<DeliveryFee>
{
    public void Configure(EntityTypeBuilder<DeliveryFee> builder)
    {
        builder.HasKey(df => df.Id);

        builder.Property(df => df.Fee)
            .IsRequired();

        builder.Property(df => df.OrganizationId)
            .HasMaxLength(64);

        builder.Property(df => df.CityId)
            .IsRequired();
    }
}
