using System.Linq.Expressions;
using Domain.Stores;

namespace Application.Stores;

public sealed record SimpleStoreResponse(
    Guid Id,
    string Name,
    string Slug,
    string? CustomDomain,
    string? LogoUrl,
    string? Address,
    string? Phone,
    string? Email,
    string? WompiPublicKey,
    bool IsConnectedToMercadoPago)
{
    public static Expression<Func<Store, SimpleStoreResponse>> Map => store =>
        new SimpleStoreResponse(
            store.Id,
            store.Name,
            store.Slug,
            store.CustomDomain,
            store.LogoUrl,
            store.Address,
            store.Phone,
            store.Email,
            store.WompiPublicKey,
            store.IsConnectedToMercadoPago
        );
}
