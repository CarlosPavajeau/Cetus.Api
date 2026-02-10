using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentLinks;

internal sealed class PaymentLinkConfiguration : IEntityTypeConfiguration<PaymentLink>
{
    public void Configure(EntityTypeBuilder<PaymentLink> builder)
    {
        builder.HasKey(pl => pl.Id);

        builder.HasOne(pl => pl.Order)
            .WithMany()
            .HasForeignKey(pl => pl.OrderId);
    }
}
