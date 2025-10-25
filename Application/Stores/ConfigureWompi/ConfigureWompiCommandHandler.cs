using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.ConfigureWompi;

internal sealed class ConfigureWompiCommandHandler(IApplicationDbContext db, ITenantContext tenant)
    : ICommandHandler<ConfigureWompiCommand>
{
    public async Task<Result> Handle(ConfigureWompiCommand command, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .Where(s => s.Id == tenant.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure(StoreErrors.NotFoundById(tenant.Id));
        }

        store.WompiPublicKey = command.PublicKey;
        store.WompiPrivateKey = command.PrivateKey;
        store.WompiEventsKey = command.EventsKey;
        store.WompiIntegrityKey = command.IntegrityKey;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
