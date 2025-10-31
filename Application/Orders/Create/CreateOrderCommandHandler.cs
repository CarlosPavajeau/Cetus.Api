using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Application.Orders.DeliveryFees.Find;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.Create;

internal sealed class CreateOrderCommandHandler(
    IApplicationDbContext context,
    ITenantContext tenant,
    ILogger<CreateOrderCommandHandler> logger,
    IStockReservationService stockReservationService,
    HybridCache cache
) : ICommandHandler<CreateOrderCommand, SimpleOrderResponse>
{
    public async Task<Result<SimpleOrderResponse>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await UpsertCustomer(request.Customer, cancellationToken);

            var items = request.Items;
            var quantitiesByVariant = items
                .GroupBy(i => i.VariantId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var productsResult = await ValidateAndGetProducts(items, quantitiesByVariant, cancellationToken);

            if (productsResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(productsResult.Error);
            }

            var order = await CreateOrderEntity(request, customer.Id, productsResult.Value, cancellationToken);

            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber, order.StoreId));

            await context.Orders.AddAsync(order, cancellationToken);

            var reserveResult =
                await stockReservationService.TryReserveAsync(quantitiesByVariant, tenant.Id, cancellationToken);

            if (!reserveResult.Success)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<SimpleOrderResponse>(OrderErrors.InsufficientStock(
                    reserveResult.FailedVariantIds.Select(id => id.ToString()).ToList(),
                    reserveResult.FailedVariantIds.Select(id => $"{id} (requested: {quantitiesByVariant[id]})")
                        .ToList()));
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}",
                order.Id, customer.Id);

            return SimpleOrderResponse.From(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order for customer {CustomerId}", request.Customer.Id);
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<SimpleOrderResponse>(OrderErrors.CreationFailed(request.Customer.Id,
                "Unexpected error while creating order."));
        }
    }

    private async Task<Customer> UpsertCustomer(CreateOrderCustomer orderCustomer,
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

    private sealed record VariantInfo(long Id, decimal Price, string ProductName);

    private async Task<Result<List<VariantInfo>>> ValidateAndGetProducts(
        IReadOnlyList<CreateOrderItem> items,
        Dictionary<long, int> quantitiesByVariant,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return Result.Failure<List<VariantInfo>>(OrderErrors.EmptyOrder());
        }

        if (items.Any(i => i.Quantity <= 0))
        {
            return Result.Failure<List<VariantInfo>>(OrderErrors.InvalidItemQuantities());
        }

        var variantIds = quantitiesByVariant.Keys.ToList();

        var variants = await context.ProductVariants
            .AsNoTracking()
            .Where(v =>
                variantIds.Contains(v.Id) &&
                v.DeletedAt == null &&
                v.Product != null &&
                v.Product.DeletedAt == null &&
                v.Product.StoreId == tenant.Id)
            .Select(v => new VariantInfo(v.Id, v.Price, v.Product!.Name))
            .ToListAsync(cancellationToken);

        var foundVariantIds = variants.Select(p => p.Id).ToHashSet();
        var missingProducts = variantIds.Except(foundVariantIds).ToList();

        if (missingProducts.Count != 0)
        {
            var productCodes = missingProducts.Select(p => p.ToString()).ToList();
            return Result.Failure<List<VariantInfo>>(OrderErrors.ProductsNotFound(productCodes));
        }

        return variants;
    }

    private async Task<Order> CreateOrderEntity(CreateOrderCommand request, string customerId,
        IReadOnlyList<VariantInfo> variants, CancellationToken cancellationToken)
    {
        var deliveryFee = await CalculateDeliveryFee(request.CityId, tenant.Id, cancellationToken);

        var variantById = variants.ToDictionary(v => v.Id);
        var items = request.Items
            .Select(i =>
            {
                var variant = variantById[i.VariantId];
                return new OrderItem
                {
                    ProductName = variant.ProductName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    Price = variant.Price,
                    VariantId = i.VariantId
                };
            })
            .ToList();

        var subtotal = items.Sum(x => x.Price * x.Quantity);
        const decimal discount = 0m;

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
        var cacheKey = $"{cityId}-{tenantId}";

        var deliveryFee = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await context.DeliveryFees
                .AsNoTracking()
                .Where(x => x.CityId == cityId && x.StoreId == tenantId && x.DeletedAt == null)
                .Select(x => new {x.Fee})
                .FirstOrDefaultAsync(token),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1),
                LocalCacheExpiration = TimeSpan.FromHours(1)
            },
            cancellationToken: cancellationToken
        );

        return deliveryFee?.Fee ?? DeliveryFeeResponse.Empty.Fee;
    }
}
