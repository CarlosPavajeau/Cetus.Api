using System.Collections.Immutable;
using Cetus.Domain;
using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly CetusDbContext _context;

    public CreateOrderCommandHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Check if customer exists otherwise create a new one
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Customer.Id, cancellationToken);

        if (customer is null)
        {
            var newCustomer = new Customer
            {
                Id = request.Customer.Id,
                Name = request.Customer.Name,
                Email = request.Customer.Email,
                Phone = request.Customer.Phone,
                Address = request.Customer.Address
            };

            await _context.Customers.AddAsync(newCustomer, cancellationToken);
        }

        // Validate product stocks
        var items = request.Items.ToImmutableList();
        var isValid = await ValidateProductStocks(items, cancellationToken);
        if (!isValid)
        {
            throw new Exception("One or more products are out of stock.");
        }

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Address = request.Address,
            CityId = request.CityId,
            DeliveryFee =
                request.CityId.ToString() == "f97957e9-d820-4858-ac26-b5d03d658370"
                    ? 5000
                    : 15000, // TODO: Change this to a more dynamic way, maybe a delivery fee table
            Total = request.Total,
            CustomerId = request.Customer.Id,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                Price = i.Price,
                ProductId = i.ProductId
            }).ToList()
        };

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await _context.Orders.AddAsync(order, cancellationToken);
        await UpdateProductStocks(items, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return order.Id;
    }

    private async Task<bool> ValidateProductStocks(ImmutableList<CreateOrderItem> items,
        CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return products.All(p => items.Any(i => i.ProductId == p.Id && p.Stock >= i.Quantity));
    }

    private async Task UpdateProductStocks(ImmutableList<CreateOrderItem> items, CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var item = items.First(i => i.ProductId == product.Id);
            product.Stock -= item.Quantity;
        }
    }
}
