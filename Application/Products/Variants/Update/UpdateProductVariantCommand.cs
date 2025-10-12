using Application.Abstractions.Messaging;

namespace Application.Products.Variants.Update;

public sealed record UpdateProductVariantCommand(long Id, int Stock, decimal Price)
    : ICommand<SimpleProductVariantResponse>;
