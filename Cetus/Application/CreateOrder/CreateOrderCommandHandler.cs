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

        // Create order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Address = request.Address,
            Total = request.Total,
            CustomerId = request.Customer.Id,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price,
                ProductId = i.ProductId
            }).ToList()
        };

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
