using System.Linq.Expressions;
using Cetus.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cetus.Infrastructure.Persistence.EntityFramework;

public class CetusDbContext(DbContextOptions<CetusDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    private class DateTimeToUtcConverter() : ValueConverter<DateTime, DateTime>(Serialize, Deserialize)
    {
        static Expression<Func<DateTime, DateTime>> Deserialize = 
            x => x.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(x, DateTimeKind.Utc) : x;
        static Expression<Func<DateTime, DateTime>> Serialize = x => x;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeToUtcConverter>();
    }
}
