using System.Linq.Expressions;
using Domain.Stores;

namespace Application.Stores;

public sealed record StoreResponse(
    Guid Id,
    string Name,
    string Slug,
    string? CustomDomain,
    string? LogoUrl,
    string? Address,
    string? Phone,
    string? Email,
    bool IsConnectedToMercadoPago)
{
    public static Expression<Func<Store, StoreResponse>> Map => store =>
        new StoreResponse(
            store.Id,
            store.Name,
            store.Slug,
            $"https://{store.CustomDomain}",
            store.LogoUrl,
            store.Address,
            store.Phone,
            store.Email,
            store.IsConnectedToMercadoPago
        );
}
