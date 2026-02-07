using SharedKernel;

namespace Domain.Stores;

public static class StoreErrors
{
    public static Error NotFoundBySlug(string Slug) =>
        Error.NotFound(
            "Stores.NotFound",
            $"Store with Slug '{Slug}' was not found."
        );

    public static Error NotFoundByCustomDomain(string CustomDomain) =>
        Error.NotFound(
            "Stores.NotFound",
            $"Store with CustomDomain '{CustomDomain}' was not found."
        );

    public static Error NotFoundById(Guid Id) =>
        Error.NotFound(
            "Stores.NotFoundById",
            $"Store with Id '{Id}' was not found."
        );

    public static Error NotFoundByExternalId(string ExternalId) =>
        Error.NotFound(
            "Stores.NotFoundByExternalId",
            $"Store with ExternalId '{ExternalId}' was not found."
        );

    public static Error InvalidQuery() =>
        Error.Problem(
            "Stores.InvalidQuery",
            "Either CustomDomain or Slug must be provided."
        );

    public static Error AlreadyExists(string Slug) =>
        Error.Conflict(
            "Stores.AlreadyExists",
            $"Store with Slug '{Slug}' already exists."
        );

    public static Error NotConnectedToMercadoPago(string Slug) =>
        Error.Problem(
            "Stores.NotConnectedToMercadoPago",
            $"Store with Slug '{Slug}' is not connected to Mercado Pago"
        );

    public static Error WompiPublicKeyNotFound(Guid storeId) =>
        Error.Problem(
            "Stores.WompiPublicKeyNotFound",
            $"Wompi public key not found for store with Id '{storeId}'"
        );
}
