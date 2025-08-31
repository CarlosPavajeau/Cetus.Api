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
    ITenantContext tenant,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await GetOrCreateCustomer(request.Customer, cancellationToken);

            var items = request.Items;
            var productsResult = await ValidateAndGetProducts(items, cancellationToken);

            if (productsResult.IsFailure)
            {
                return Result.Failure<OrderResponse>(productsResult.Error);
            }

            var order = await CreateOrderEntity(request, customer.Id, productsResult.Value, cancellationToken);

            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber));

            await context.Orders.AddAsync(order, cancellationToken);

            UpdateProductStocks(productsResult.Value, items);

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
            return Result.Failure<OrderResponse>(OrderErrors.CreationFailed(request.Customer.Id, ex.Message));
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

    private async Task<Result<List<ProductVariant>>> ValidateAndGetProducts(IReadOnlyList<CreateOrderItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return Result.Failure<List<ProductVariant>>(OrderErrors.EmptyOrder());
        }

        if (items.Any(i => i.Quantity <= 0))
        {
            return Result.Failure<List<ProductVariant>>(OrderErrors.InvalidItemQuantities());
        }

        var quantitiesByVariant = items
            .GroupBy(i => i.VariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var variantIds = quantitiesByVariant.Keys.ToList();
        var products = await context.ProductVariants
            .Include(v => v.Product)
            .Where(v =>
                variantIds.Contains(v.Id) &&
                v.DeletedAt == null &&
                v.Product != null &&
                v.Product.DeletedAt == null &&
                v.Product.StoreId == tenant.Id)
            .ToListAsync(cancellationToken);

        var missingProducts = variantIds.Except(products.Select(p => p.Id)).ToList();
        if (missingProducts.Count != 0)
        {
            var productCodes = missingProducts.Select(p => p.ToString()).ToList();
            return Result.Failure<List<ProductVariant>>(OrderErrors.ProductsNotFound(productCodes));
        }

        var outOfStockProducts = products
            .Where(p => p.StockQuantity < quantitiesByVariant[p.Id])
            .Select(p => new {p.Id, p.StockQuantity, Requested = quantitiesByVariant[p.Id]})
            .ToList();

        if (outOfStockProducts.Count == 0)
        {
            return products;
        }

        var outOfStockProductsDetails = outOfStockProducts
            .Select(p => $"{p.Id} (stock: {p.StockQuantity})")
            .ToList();

        var requestedProducts = outOfStockProducts
            .Select(p => $"{p.Id} (requested: {p.Requested})")
            .ToList();

        return Result.Failure<List<ProductVariant>>(
            OrderErrors.InsufficientStock(outOfStockProductsDetails, requestedProducts));
    }

    private async Task<Order> CreateOrderEntity(CreateOrderCommand request, string customerId,
        IReadOnlyList<ProductVariant> variants, CancellationToken cancellationToken)
    {
        var deliveryFee = await CalculateDeliveryFee(request.CityId, tenant.Id, cancellationToken);

        var variantById = variants.ToDictionary(v => v.Id);
        var items = request.Items
            .Select(i =>
            {
                var variant = variantById[i.VariantId];
                return new OrderItem
                {
                    ProductName = variant.Product?.Name ?? i.ProductName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    Price = variant.Price, // authoritative price
                    VariantId = i.VariantId
                };
            })
            .ToList();

        var subtotal = items.Sum(x => x.Price * x.Quantity);
        const decimal discount = 0m; // placeholder for future promotions

        return new Order
        {
            Id = Guid.NewGuid(),
            Address = request.Address,
            CityId = request.CityId,
            Subtotal = subtotal,
            Discount = discount,
            DeliveryFee = deliveryFee,
            Total = subtotal + deliveryFee - discount,
            CustomerId = customerId,
            StoreId = tenant.Id,
            Items = items
        };
    }

    private async Task<decimal> CalculateDeliveryFee(Guid cityId, Guid tenantId, CancellationToken cancellationToken)
    {
        var deliveryFee = await context.DeliveryFees
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.CityId == cityId && x.StoreId == tenantId && x.DeletedAt == null,
                cancellationToken
            );

        return deliveryFee?.Fee ?? DeliveryFeeResponse.Empty.Fee;
    }

    private static void UpdateProductStocks(List<ProductVariant> products, IReadOnlyList<CreateOrderItem> items)
    {
        var quantitiesByVariant = items
            .GroupBy(i => i.VariantId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        foreach (var product in products)
        {
            if (quantitiesByVariant.TryGetValue(product.Id, out var qty))
            {
                product.StockQuantity -= qty;
            }
        }
    }
}
