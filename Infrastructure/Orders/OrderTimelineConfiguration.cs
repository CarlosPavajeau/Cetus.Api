using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Orders;

internal sealed class OrderTimelineConfiguration : IEntityTypeConfiguration<OrderTimeline>
{
    public void Configure(EntityTypeBuilder<OrderTimeline> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(o => o.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.OrderId);
    }
}
