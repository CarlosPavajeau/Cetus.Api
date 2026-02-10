using Domain.PaymentLinks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.PaymentLinks;

internal sealed class PaymentLinkConfiguration : IEntityTypeConfiguration<PaymentLink>
{
    public void Configure(EntityTypeBuilder<PaymentLink> builder)
    {
        builder.HasKey(pl => pl.Id);

        builder.Property(pl => pl.Token).HasMaxLength(64).IsRequired();
        builder.HasIndex(pl => pl.Token).IsUnique();

        builder.Property(pl => pl.Status)
            .IsRequired();

        builder.Property(pl => pl.ExpiresAt)
            .IsRequired();

        builder.Property(pl => pl.CreatedAt)
            .IsRequired();

        builder.HasOne(pl => pl.Order)
            .WithMany()
            .HasForeignKey(pl => pl.OrderId);
    }
}
