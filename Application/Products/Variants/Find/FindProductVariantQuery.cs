using Application.Abstractions.Messaging;

namespace Application.Products.Variants.Find;

public sealed record FindProductVariantQuery(long Id) : IQuery<ProductVariantResponse>;
