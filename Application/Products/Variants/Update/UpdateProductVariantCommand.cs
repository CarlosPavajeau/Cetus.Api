using Application.Abstractions.Messaging;

namespace Application.Products.Variants.Update;

public sealed record UpdateProductVariantCommand(
    long Id,
    decimal Price,
    bool Enabled,
    bool Featured,
    decimal? CostPrice = null,
    decimal? CompareAtPrice = null
) : ICommand<SimpleProductVariantResponse>;
