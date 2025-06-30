using SharedKernel;

namespace Domain.Stores;

public static class StoreErrors
{
    public static Error NotFound(string? CustomDomain, string? Slug) =>
        Error.NotFound(
            "Stores.NotFound",
            $"Store with CustomDomain '{CustomDomain}' or Slug '{Slug}' was not found."
        );

    public static Error InvalidQuery() =>
        Error.Problem(
            "Stores.InvalidQuery",
            "Either CustomDomain or Slug must be provided."
        );
}
