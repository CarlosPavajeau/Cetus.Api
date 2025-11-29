using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Stores;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Stores.Create;

internal sealed class CreateStoreCommandHandler(IApplicationDbContext db, IDateTimeProvider timeProvider)
    : ICommandHandler<CreateStoreCommand>
{
    public async Task<Result> Handle(CreateStoreCommand command, CancellationToken cancellationToken)
    {
        bool alreadyExists = await db.Stores.AnyAsync(x => x.Slug == command.Slug, cancellationToken);
        if (alreadyExists)
        {
            return Result.Failure(StoreErrors.AlreadyExists(command.Slug));
        }

        var now = timeProvider.UtcNow;
        var store = new Store
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Slug = command.Slug,
            ExternalId = command.ExternalId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await db.Stores.AddAsync(store, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
