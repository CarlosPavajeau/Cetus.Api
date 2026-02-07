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
            return Result.Failure<StoreResponse>(StoreErrors.NotFoundById(command.Id));
        }

        store.Name = command.Name;

        if (!string.IsNullOrWhiteSpace(command.Description))
        {
            store.Description = command.Description;
        }

        if (!string.IsNullOrWhiteSpace(command.Address))
        {
            store.Address = command.Address;
        }

        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            store.Phone = command.Phone;
        }

        if (!string.IsNullOrWhiteSpace(command.Email))
        {
            store.Email = command.Email;
        }

        if (!string.IsNullOrWhiteSpace(command.CustomDomain))
        {
            store.CustomDomain = command.CustomDomain;
        }

        await db.SaveChangesAsync(cancellationToken);

        return StoreResponse.Map.Compile()(store);
    }
}
