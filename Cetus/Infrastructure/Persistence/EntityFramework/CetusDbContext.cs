using Cetus.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Infrastructure.Persistence.EntityFramework;

public class CetusDbContext(DbContextOptions<CetusDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
