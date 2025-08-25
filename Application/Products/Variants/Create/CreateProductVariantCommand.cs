using Application.Abstractions.Messaging;
using Application.Products.Create;

namespace Application.Products.Variants.Create;

public sealed record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    decimal Price,
    int StockQuantity,
    IReadOnlyList<long> OptionValueIds,
    IReadOnlyList<CreateProductImage> Images
) : ICommand;
