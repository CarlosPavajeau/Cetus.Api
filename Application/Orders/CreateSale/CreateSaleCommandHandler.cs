using System.Globalization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services;
using Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Orders.CreateSale;

internal sealed class CreateSaleCommandHandler(
    IApplicationDbContext db,
    ITenantContext tenant,
    ILogger<CreateSaleCommandHandler> logger,
    IStockReservationService stockReservationService
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

            var productsResult = await ValidateAndGetProducts(items, quantitiesByVariant, cancellationToken);

            if (productsResult.IsFailure)
            {
                return Result.Failure<SimpleOrderResponse>(productsResult.Error);
            }

            var order = CreateOrderEntity(command, customer.Id, productsResult.Value);

            order.Raise(new OrderCreatedDomainEvent(order.Id, order.OrderNumber, order.StoreId));

            await db.Orders.AddAsync(order, cancellationToken);

            var reserveResult =
                await stockReservationService.TryReserveAsync(quantitiesByVariant, order.Id, tenant.Id,
                    cancellationToken);

            if (!reserveResult.Success)
            {
                await transaction.RollbackAsync(cancellationToken);

                var variantsById = productsResult.Value.ToDictionary(v => v.Id);
                var outOfStockProducts = reserveResult.FailedVariantIds
                    .Select(id =>
                        variantsById.TryGetValue(id, out var variant)
                            ? variant.ProductName
                            : id.ToString(CultureInfo.InvariantCulture))
                    .ToList();

                var requestedProducts = reserveResult.FailedVariantIds
                    .Select(id =>
                    {
                        string label = variantsById.TryGetValue(id, out var variant)
                            ? variant.ProductName
                            : id.ToString(CultureInfo.InvariantCulture);
                        string quantity = quantitiesByVariant.TryGetValue(id, out int qty) ? $"{qty}" : "unknown";

                        return $"{label} (requested: {quantity})";
                    })
                    .ToList();


                return Result.Failure<SimpleOrderResponse>(
                    OrderErrors.InsufficientStock(outOfStockProducts, requestedProducts));
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
        var customer = await db.Customers
            .FirstOrDefaultAsync(c => c.Phone == saleCustomer.Phone, cancellationToken);

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
            Phone = saleCustomer.Phone
        };

        await db.Customers.AddAsync(customer, cancellationToken);

        logger.LogInformation("New customer {CustomerId} created", customer.Id);

        return customer;
    }

    private sealed record VariantInfo(long Id, decimal Price, string ProductName, string ImageUrl = "");

    private async Task<Result<List<VariantInfo>>> ValidateAndGetProducts(
        IReadOnlyList<CreateSaleItem> items,
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

        var variants = await db.ProductVariants
            .AsNoTracking()
            .Where(v =>
                variantIds.Contains(v.Id) &&
                v.DeletedAt == null &&
                v.Product != null &&
                v.Product.DeletedAt == null &&
                v.Product.StoreId == tenant.Id)
            .Select(v => new VariantInfo(
                v.Id,
                v.Price,
                v.Product!.Name,
                v.Images.FirstOrDefault()!.ImageUrl
            ))
            .ToListAsync(cancellationToken);

        var foundVariantIds = variants.Select(p => p.Id).ToHashSet();
        var missingProducts = variantIds.Except(foundVariantIds).ToList();

        if (missingProducts.Count != 0)
        {
            var productCodes = missingProducts.Select(p => p.ToString(CultureInfo.InvariantCulture)).ToList();
            return Result.Failure<List<VariantInfo>>(OrderErrors.ProductsNotFound(productCodes));
        }

        return variants;
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
            Id = Guid.NewGuid(),
            Address = command.Shipping?.Address,
            CityId = command.Shipping?.CityId,
            Channel = command.Channel,
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
