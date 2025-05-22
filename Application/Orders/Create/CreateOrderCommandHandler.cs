using System.Collections.Immutable;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.DeliveryFees.Find;
using Application.Orders.SearchAll;
using Domain.Orders;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await GetOrCreateCustomer(request.Customer, cancellationToken);

            var items = request.Items.ToImmutableList();
            var productsResult = await ValidateAndGetProducts(items, cancellationToken);

            if (productsResult.IsFailure)
            {
                return Result.Failure<OrderResponse>(productsResult.Error);
            }

            var order = await CreateOrderEntity(request, customer.Id);
            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber));
            await context.Orders.AddAsync(order, cancellationToken);

            var products = productsResult.Value;
            UpdateProductStocks(products, items);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}",
                order.Id, customer.Id);

            return OrderResponse.FromOrder(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order for customer {CustomerId}", request.Customer.Id);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Customer> GetOrCreateCustomer(CreateOrderCustomer orderCustomer,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
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

        await context.Customers.AddAsync(customer, cancellationToken);
        logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private async Task<Result<List<Product>>> ValidateAndGetProducts(ImmutableList<CreateOrderItem> items,
        CancellationToken cancellationToken)
    {
        var productIds = items.Select(i => i.ProductId).ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missingProducts.Count != 0)
        {
            var productCodes = missingProducts.Select(p => p.ToString()).ToList();
            return Result.Failure<List<Product>>(OrderErrors.ProductsNotFound(productCodes));
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

        return Result.Failure<List<Product>>(
            OrderErrors.InsufficientStock(outOfStockProductsDetails, requestedProducts));
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
        var deliveryFee = await context.DeliveryFees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CityId == cityId);

        return deliveryFee?.Fee ?? DeliveryFeeResponse.Empty.Fee;
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
