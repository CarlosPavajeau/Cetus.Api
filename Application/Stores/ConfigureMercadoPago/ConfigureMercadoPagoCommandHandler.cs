using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using SharedKernel;

namespace Application.Stores.ConfigureMercadoPago;

internal sealed class ConfigureMercadoPagoCommandHandler(IApplicationDbContext db, ITenantContext tenant, HybridCache cache)
    : ICommandHandler<ConfigureMercadoPagoCommand>
{
    public async Task<Result> Handle(ConfigureMercadoPagoCommand command, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .Where(s => s.Id == tenant.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure(StoreErrors.NotFound(tenant.Id.ToString(), null));
        }
        
        store.MercadoPagoAccessToken = command.AccessToken;
        store.MercadoPagoRefreshToken = command.RefreshToken;
        store.MercadoPagoExpiresAt = DateTime.Today.AddSeconds(command.ExpiresIn).ToUniversalTime();
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Clear the cache for the store
        await cache.RemoveAsync($"store-${store.CustomDomain}", cancellationToken);
        await cache.RemoveAsync($"store-${store.Slug}", cancellationToken);
        
        return Result.Success();
    }
}
