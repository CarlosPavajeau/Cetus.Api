using SharedKernel;

namespace Domain.Products;

public static class ProductOptionErrors
{
    public static Error CrossStoreAssociation() => Error.Conflict(
        "ProductOption.CrossStoreAssociation",
        "Product option cannot be associated with a product from a different store."
    );
}
