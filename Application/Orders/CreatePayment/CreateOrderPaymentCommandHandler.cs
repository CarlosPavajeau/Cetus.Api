using Application.Abstractions.Data;
using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Stores;
using MercadoPago.Client.Preference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Application.Orders.CreatePayment;

internal sealed class CreateOrderPaymentCommandHandler(
    IApplicationDbContext db,
    IMercadoPagoClient mercadoPagoClient,
    IConfiguration configuration
) : ICommandHandler<CreateOrderPaymentCommand, string>
{
    public async Task<Result<string>> Handle(CreateOrderPaymentCommand command, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Where(o => o.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<string>(OrderErrors.NotFound(command.Id));
        }

        if (!order.CanTransitionTo(OrderStatus.PaymentConfirmed))
        {
            return Result.Failure<string>(
                OrderErrors.InvalidStatusTransition(order.Status, OrderStatus.PaymentConfirmed));
        }

        var store = await db.Stores
            .AsNoTracking()
            .Where(s => s.Id == order.StoreId)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<string>(StoreErrors.NotFoundById(order.StoreId));
        }

        if (!store.IsConnectedToMercadoPago)
        {
            return Result.Failure<string>(StoreErrors.NotConnectedToMercadoPago(store.Slug));
        }

        string? cdnUrl = configuration["CdnUrl"];
        var createPreferenceRequest = new PreferenceRequest
        {
            ExternalReference = order.Id.ToString(),
            Payer = new PreferencePayerRequest
            {
                Name = order.Customer!.Name,
                Email = order.Customer.Email
            },
            Items = order.Items.Select(item => new PreferenceItemRequest
            {
                Id = item.Id.ToString(),
                Title = item.ProductName,
                Description = item.ProductName,
                PictureUrl = $"{cdnUrl}/{item.ImageUrl}",
                Quantity = item.Quantity,
                UnitPrice = item.Price
            }).ToList(),
            MarketplaceFee = CalculateFee(order.Subtotal)
        };

        if (store.CustomDomain is not null)
        {
            string backUrl =
                $"https://{store.CustomDomain}/orders/{createPreferenceRequest.ExternalReference}/confirmation";
            createPreferenceRequest.BackUrls = new PreferenceBackUrlsRequest
            {
                Success = backUrl,
                Failure = backUrl,
                Pending = backUrl
            };
        }

        var preference =
            await mercadoPagoClient.CreatePreference(createPreferenceRequest, store.MercadoPagoAccessToken!,
                cancellationToken);

        if (preference is null)
        {
            return Result.Failure<string>(OrderErrors.PaymentCreationFailed(command.Id));
        }

        return Result.Success(preference.InitPoint);
    }

    private static decimal CalculateFee(decimal total)
    {
        // Now, this app charge only 2% of the total as a fee
        const decimal feePercentage = 0.02m;
        return total * feePercentage;
    }
}
