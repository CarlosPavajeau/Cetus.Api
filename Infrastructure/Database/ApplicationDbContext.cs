using System.Linq.Expressions;
using Application.Abstractions.Data;
using Domain.Auth;
using Domain.Categories;
using Domain.Coupons;
using Domain.Orders;
using Domain.Products;
using Domain.Reviews;
using Domain.States;
using Domain.Stores;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductOptionType> ProductOptionTypes { get; set; }
    public DbSet<ProductOptionValue> ProductOptionValues { get; set; }
    public DbSet<ProductOption> ProductOptions { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductVariantOptionValue> ProductVariantOptionValues { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<DeliveryFee> DeliveryFees { get; set; }

    public DbSet<State> States { get; set; }
    public DbSet<City> Cities { get; set; }

    public DbSet<ReviewRequest> ReviewRequests { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }

    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponRule> CouponRules { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }

    public DbSet<Store> Stores { get; set; }

    public DbSet<User> Users { get; set; }

    private sealed class DateTimeToUtcConverter() : ValueConverter<DateTime, DateTime>(Serialize, Deserialize)
    {
        static Expression<Func<DateTime, DateTime>> Deserialize =
            x => x.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(x, DateTimeKind.Utc) : x;

        static Expression<Func<DateTime, DateTime>> Serialize =
            x => x.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(x, DateTimeKind.Utc) : x;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeToUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
