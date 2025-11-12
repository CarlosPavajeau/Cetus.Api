using Domain.Auth;
using Domain.Categories;
using Domain.Coupons;
using Domain.Orders;
using Domain.Products;
using Domain.Reviews;
using Domain.States;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }

    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductOptionType> ProductOptionTypes { get; }
    DbSet<ProductOptionValue> ProductOptionValues { get; }
    DbSet<ProductOption> ProductOptions { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductVariantOptionValue> ProductVariantOptionValues { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<DeliveryFee> DeliveryFees { get; }

    DbSet<State> States { get; }
    DbSet<City> Cities { get; }

    DbSet<ReviewRequest> ReviewRequests { get; }
    DbSet<ProductReview> ProductReviews { get; }

    DbSet<Coupon> Coupons { get; }
    DbSet<CouponRule> CouponRules { get; }
    DbSet<CouponUsage> CouponUsages { get; }

    DbSet<Store> Stores { get; }

    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
