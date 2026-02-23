using Application.Abstractions.Configurations;
using Application.Abstractions.Data;
using Application.Abstractions.MercadoPago;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Stores;
using MercadoPago.Client.Preference;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.PaymentProviders.MercadoPago;

internal sealed class CreateMercadoPagoPreferenceCommandHandler(
    IApplicationDbContext db,
    IMercadoPagoClient mercadoPagoClient,
    IOptions<AppSettings> options,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<CreateMercadoPagoPreferenceCommand, string>
{
    private readonly AppSettings _appSettings = options.Value;

    public async Task<Result<string>> Handle(CreateMercadoPagoPreferenceCommand command,
        CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Where(o => o.Id == command.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            return Result.Failure<string>(OrderErrors.NotFound(command.OrderId));
        }

        if (!order.CanTransitionTo(OrderStatus.PaymentConfirmed))
        {
            return Result.Failure<string>(
                OrderErrors.InvalidStatusTransition(order.Status, OrderStatus.PaymentConfirmed)
            );
        }

        var store = await db.Stores
            .Where(s => s.Id == order.StoreId)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<string>(StoreErrors.NotFoundById(order.StoreId));
        }

        if (string.IsNullOrEmpty(store.MercadoPagoAccessToken))
        {
            return Result.Failure<string>(StoreErrors.NotConnectedToMercadoPago(store.Slug));
        }

        const int tokenExpirationBufferMinutes = 5;
        string accessToken = store.MercadoPagoAccessToken!;

        if (store.MercadoPagoExpiresAt.HasValue &&
            store.MercadoPagoExpiresAt.Value <= dateTimeProvider.UtcNow.AddMinutes(tokenExpirationBufferMinutes))
        {
            var tokenResponse = await mercadoPagoClient.RefreshAccessTokenAsync(
                store.MercadoPagoRefreshToken!, cancellationToken);

            if (tokenResponse is null)
            {
                return Result.Failure<string>(StoreErrors.MercadoPagoTokenRefreshFailed(order.StoreId));
            }

            store.MercadoPagoAccessToken = tokenResponse.AccessToken;
            store.MercadoPagoRefreshToken = tokenResponse.RefreshToken;
            store.MercadoPagoExpiresAt = dateTimeProvider.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            accessToken = tokenResponse.AccessToken;

            await db.SaveChangesAsync(cancellationToken);
        }

        string cdnUrl = _appSettings.CdnUrl;
        var createPreferenceRequest = new PreferenceRequest
        {
            ExternalReference = order.Id.ToString(),
            Payer = new PreferencePayerRequest
            {
                Name = order.Customer!.Name,
                Email = order.Customer.Email
            },
            Items =
            [
                .. order.Items.Select(item => new PreferenceItemRequest
                {
                    Id = item.Id.ToString(),
                    Title = item.ProductName,
                    Description = item.ProductName,
                    PictureUrl = $"{cdnUrl}/{item.ImageUrl}",
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                })
            ],
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

        var preference = await mercadoPagoClient.CreatePreference(
            createPreferenceRequest,
            accessToken,
            cancellationToken
        );

        if (preference is null)
        {
            return Result.Failure<string>(OrderErrors.PaymentCreationFailed(command.OrderId));
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
