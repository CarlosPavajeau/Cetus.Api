using System.Collections.Immutable;
using Cetus.Infrastructure.Persistence.EntityFramework;
using Cetus.Orders.Application.DeliveryFees.Find;
using Cetus.Orders.Application.SearchAll;
using Cetus.Orders.Domain;
using Cetus.Orders.Domain.Events;
using Cetus.Products.Domain;
using Cetus.Shared.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cetus.Orders.Application.Create;

internal sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly CetusDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(CetusDbContext context, ILogger<CreateOrderCommandHandler> logger,
        IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await GetOrCreateCustomer(request.Customer, cancellationToken);

            var items = request.Items.ToImmutableList();
            var products = await ValidateAndGetProducts(items, cancellationToken);

            var order = await CreateOrderEntity(request, customer.Id);
            await _context.Orders.AddAsync(order, cancellationToken);

            UpdateProductStocks(products, items);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}",
                order.Id, customer.Id);

            await _mediator.Publish(
                new OrderCreatedEvent(new OrderCreated(order.Id, order.OrderNumber)),
                cancellationToken
            );

            return OrderResponse.FromOrder(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.Customer.Id);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Customer> GetOrCreateCustomer(CreateOrderCustomer orderCustomer,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == orderCustomer.Id, cancellationToken);

        if (customer is not null)
        {
            return customer;
        }

        customer = new Customer
        {
            Id = orderCustomer.Id,
            Name = orderCustomer.Name,
            Email = orderCustomer.Email,
            Phone = orderCustomer.Phone,
            Address = orderCustomer.Address
        };

        await _context.Customers.AddAsync(customer, cancellationToken);
        _logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private async Task<List<Product>> ValidateAndGetProducts(ImmutableList<CreateOrderItem> items,
        CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missingProducts.Count != 0)
        {
            throw new ProductNotFoundException($"Products not found: {string.Join(", ", missingProducts)}");
        }

        var outOfStockProducts = products
            .Where(p => !items.Any(i => i.ProductId == p.Id && p.Stock >= i.Quantity))
            .Select(p => new {p.Id, p.Stock})
            .ToList();

        if (outOfStockProducts.Count == 0)
        {
            return products;
        }

        var outOfStockProductsDetails = outOfStockProducts
            .Select(p => $"{p.Id} (stock: {p.Stock})")
            .ToList();

        var requestedProducts = items
            .Where(i => outOfStockProducts.Any(p => p.Id == i.ProductId))
            .Select(i => $"{i.ProductId} (requested: {i.Quantity})")
            .ToList();

        throw new InsufficientStockException(
            $"Insufficient stock for products: {string.Join(", ", outOfStockProductsDetails)}. " +
            $"Requested quantities: {string.Join(", ", requestedProducts)}");
    }

    private async Task<Order> CreateOrderEntity(CreateOrderCommand request, string customerId)
    {
        var deliveryFee = await CalculateDeliveryFee(request.CityId);

        return new Order
        {
            Id = Guid.NewGuid(),
            Address = request.Address,
            CityId = request.CityId,
            DeliveryFee = deliveryFee,
            Total = request.Total,
            CustomerId = customerId,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                ImageUrl = i.ImageUrl,
                Quantity = i.Quantity,
                Price = i.Price,
                ProductId = i.ProductId
            }).ToList()
        };
    }

    private async Task<decimal> CalculateDeliveryFee(Guid cityId)
    {
        var deliveryFee = await _mediator.Send(new FindDeliveryFeeQuery(cityId));
        return deliveryFee.Fee;
    }

    private static void UpdateProductStocks(List<Product> products, ImmutableList<CreateOrderItem> items)
    {
        foreach (var product in products)
        {
            var item = items.First(i => i.ProductId == product.Id);
            product.Stock -= item.Quantity;
        }
    }
}
