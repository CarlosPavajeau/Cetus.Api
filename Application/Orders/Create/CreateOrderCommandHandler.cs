using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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
    OrderCreationService orderCreationService,
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

            var variantIds = quantitiesByVariant.Keys.ToList();
            var productsResult = await orderCreationService.ValidateAndGetVariantsAsync(
                variantIds, quantitiesByVariant, items.Count, cancellationToken);

            if (productsResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(productsResult.Error);
            }

            var order = await CreateOrderEntity(request, customer.Id, productsResult.Value,
                cancellationToken);

            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber, order.StoreId));

            await context.Orders.AddAsync(order, cancellationToken);

            var reserveResult = await orderCreationService.ReserveStockOrFailAsync(
                quantitiesByVariant, order.Id, productsResult.Value, transaction, cancellationToken);

            if (reserveResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(reserveResult.Error);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}",
                order.Id, customer.Id);

            return SimpleOrderResponse.From(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order for customer {CustomerId}", request.Customer.Phone);
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<SimpleOrderResponse>(OrderErrors.CreationFailed(request.Customer.Phone,
                "Unexpected error while creating order."));
        }
    }

    private async Task<Customer> UpsertCustomer(CreateOrderCustomer orderCustomer,
        CancellationToken cancellationToken)
    {
        string normalizedPhone = new([.. orderCustomer.Phone.Where(char.IsDigit)]);
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Phone == normalizedPhone, cancellationToken);

        if (customer is not null)
        {
            return customer;
        }

        customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            DocumentType = orderCustomer.DocumentType,
            DocumentNumber = orderCustomer.DocumentNumber,
            Name = orderCustomer.Name,
            Email = orderCustomer.Email,
            Phone = normalizedPhone
        };

        await context.Customers.AddAsync(customer, cancellationToken);
        await cache.RemoveAsync($"customer-by-phone-{normalizedPhone}", cancellationToken);

        logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private async Task<Order> CreateOrderEntity(CreateOrderCommand request, Guid customerId,
        IReadOnlyList<VariantInfo> variants, CancellationToken cancellationToken)
    {
        decimal deliveryFee = await CalculateDeliveryFee(request.Shipping.CityId, tenant.Id, cancellationToken);

        var variantById = variants.ToDictionary(v => v.Id);
        var items = request.Items
            .Select(i =>
            {
                var variant = variantById[i.VariantId];
                return new OrderItem
                {
                    ProductName = variant.ProductName,
                    ImageUrl = variant.ImageUrl,
                    Quantity = i.Quantity,
                    Price = variant.Price,
                    VariantId = i.VariantId
                };
            })
            .ToList();

        decimal subtotal = items.Sum(x => x.Price * x.Quantity);
        const decimal discount = 0m;

        return new Order
        {
            Id = Guid.CreateVersion7(),
            Address = request.Shipping.Address,
            CityId = request.Shipping.CityId,
            Channel = OrderChannel.Ecommerce,
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
        string cacheKey = $"{cityId}-{tenantId}";

        var deliveryFee = await cache.GetOrCreateAsync(
            cacheKey,
            async token => await context.DeliveryFees
                .AsNoTracking()
                .Where(x => x.CityId == cityId && x.StoreId == tenantId && x.DeletedAt == null)
                .Select(x => new { x.Fee })
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
