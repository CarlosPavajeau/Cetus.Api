using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Customers;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.CreateSale;

internal sealed class CreateSaleCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache,
    ILogger<CreateSaleCommandHandler> logger,
    OrderCreationService orderCreationService
) : ICommandHandler<CreateSaleCommand, SimpleOrderResponse>
{
    public async Task<Result<SimpleOrderResponse>> Handle(CreateSaleCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await UpsertCustomer(command.Customer, cancellationToken);
            var items = command.Items;

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

            var order = CreateOrderEntity(command, customer.Id, productsResult.Value);

            if (command.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                logger.LogInformation("Order {OrderId} payment verified automatically for Cash on Delivery",
                    order.Id);

                order.PaymentStatus = PaymentStatus.Verified;
                order.Status = OrderStatus.Processing;

                var timelineEntry = new OrderTimeline
                {
                    Id = Guid.CreateVersion7(),
                    OrderId = order.Id,
                    ToStatus = order.Status,
                    CreatedAt = dateTimeProvider.UtcNow
                };

                await db.OrderTimeline.AddAsync(timelineEntry, cancellationToken);
            }

            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber, order.StoreId));
            await db.Orders.AddAsync(order, cancellationToken);

            var reserveResult = await orderCreationService.ReserveStockOrFailAsync(
                quantitiesByVariant, order.Id, productsResult.Value, transaction, cancellationToken);

            if (reserveResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(reserveResult.Error);
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Sale {OrderId} created successfully for customer {CustomerId}",
                order.Id, customer.Id);

            return SimpleOrderResponse.From(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sale for customer {CustomerId}", command.Customer.Phone);
            await transaction.RollbackAsync(cancellationToken);

            return Result.Failure<SimpleOrderResponse>(OrderErrors.CreationFailed(command.Customer.Phone,
                "Unexpected error while creating sale."));
        }
    }

    private async Task<Customer> UpsertCustomer(CreateSaleCustomer saleCustomer,
        CancellationToken cancellationToken)
    {
        string normalizedPhone = new([.. saleCustomer.Phone.Where(char.IsDigit)]);
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Phone == normalizedPhone, cancellationToken);

        if (customer is not null)
        {
            return customer;
        }

        customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            DocumentType = saleCustomer.DocumentType,
            DocumentNumber = saleCustomer.DocumentNumber,
            Name = saleCustomer.Name,
            Email = saleCustomer.Email,
            Phone = normalizedPhone
        };

        await db.Customers.AddAsync(customer, cancellationToken);
        await cache.RemoveAsync($"customer-by-phone-{normalizedPhone}", cancellationToken);

        logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private Order CreateOrderEntity(CreateSaleCommand command, Guid customerId, IReadOnlyList<VariantInfo> variants)
    {
        var variantById = variants.ToDictionary(v => v.Id);
        var items = command.Items
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
        const decimal deliveryFee = 0m;

        return new Order
        {
            Id = Guid.CreateVersion7(),
            Address = command.Shipping?.Address,
            CityId = command.Shipping?.CityId,
            Channel = command.Channel,
            PaymentProvider = PaymentProvider.Manual,
            PaymentMethod = command.PaymentMethod,
            PaymentStatus = command.PaymentStatus,
            Subtotal = subtotal,
            Discount = discount,
            DeliveryFee = deliveryFee,
            Total = subtotal,
            CustomerId = customerId,
            StoreId = tenant.Id,
            Items = items
        };
    }
}
