using Domain.Categories;
using Domain.Orders;
using Domain.Products;
using Domain.States;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<DeliveryFee> DeliveryFees { get; }

    DbSet<State> States { get; }
    DbSet<City> Cities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
