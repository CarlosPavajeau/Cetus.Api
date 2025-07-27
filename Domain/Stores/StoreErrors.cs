using SharedKernel;

namespace Domain.Stores;

public static class StoreErrors
{
    public static Error NotFound(string? CustomDomain, string? Slug) =>
        Error.NotFound(
            "Stores.NotFound",
            $"Store with CustomDomain '{CustomDomain}' or Slug '{Slug}' was not found."
        );

    public static Error NotFoundById(string Id) =>
        Error.NotFound(
            "Stores.NotFoundById",
            $"Store with Id '{Id}' was not found."
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
}
