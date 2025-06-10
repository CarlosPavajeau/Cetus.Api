using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Reviews;

internal sealed class ReviewRequestConfiguration : IEntityTypeConfiguration<ReviewRequest>
{
    public void Configure(EntityTypeBuilder<ReviewRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(rr => rr.OrderItemId)
            .IsRequired();

        builder.Property(rr => rr.CustomerId)
            .IsRequired()
            .HasMaxLength(50);
    }
}
