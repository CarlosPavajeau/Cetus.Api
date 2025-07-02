using Domain.Stores;

namespace Application.Stores;

public sealed record StoreResponse(Guid Id, string Name, string Slug, string? CustomDomain, string? LogoUrl)
{
    public static StoreResponse FromStore(Store store)
    {
        return new StoreResponse(
            store.Id,
            store.Name,
            store.Slug,
            store.CustomDomain,
            store.LogoUrl
        );
    }
}
