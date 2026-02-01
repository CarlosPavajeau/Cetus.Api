using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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
            .FirstOrDefaultAsync(c => c.DocumentNumber == orderCustomer.Id, cancellationToken);

        if (customer is not null)
        {
            return customer;
        }

        customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            DocumentType = DocumentType.CC,
            DocumentNumber = orderCustomer.Id,
            Name = orderCustomer.Name,
            Email = orderCustomer.Email,
            Phone = orderCustomer.Phone,
            Address = orderCustomer.Address
        };

        await context.Customers.AddAsync(customer, cancellationToken);

        logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private async Task<Order> CreateOrderEntity(CreateOrderCommand request, Guid customerId,
        IReadOnlyList<VariantInfo> variants, CancellationToken cancellationToken)
    {
        decimal deliveryFee = await CalculateDeliveryFee(request.CityId, tenant.Id, cancellationToken);

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

        decimal subtotal = items.Sum(x => x.Price * x.Quantity);
        const decimal discount = 0m;

        return new Order
        {
            Id = Guid.CreateVersion7(),
            Address = request.Address,
            CityId = request.CityId,
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

        return deliveryFee?.Fee ?? DeliveryFees.Find.DeliveryFeeResponse.Empty.Fee;
    }
}
