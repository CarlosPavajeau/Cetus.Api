using Domain.Orders;
using Domain.Products;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Reviews;

internal sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pr => pr.Rating)
            .IsRequired();

        builder.Property(pr => pr.ModeratorNotes)
            .HasMaxLength(1000);

        builder.Property(pr => pr.ReviewRequestId)
            .IsRequired();
        builder.HasOne<ReviewRequest>()
            .WithOne()
            .HasForeignKey<ProductReview>(pr => pr.ReviewRequestId);

        builder.Property(pr => pr.ProductId)
            .IsRequired();
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pr => pr.CustomerId)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(pr => pr.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
