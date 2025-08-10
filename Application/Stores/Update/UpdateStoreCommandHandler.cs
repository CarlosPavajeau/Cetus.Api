using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.Update;

internal sealed class UpdateStoreCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateStoreCommand, StoreResponse>
{
    public async Task<Result<StoreResponse>> Handle(UpdateStoreCommand command, CancellationToken cancellationToken)
    {
        var store = await db.Stores
            .Where(s => s.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (store is null)
        {
            return Result.Failure<StoreResponse>(StoreErrors.NotFoundById(command.Id.ToString()));
        }

        store.Name = command.Name;
        store.Description = command.Description;
        store.Address = command.Address;
        store.Phone = command.Phone;
        store.Email = command.Email;
        store.CustomDomain = command.CustomDomain;

        await db.SaveChangesAsync(cancellationToken);

        return StoreResponse.Map.Compile()(store);
    }
}
