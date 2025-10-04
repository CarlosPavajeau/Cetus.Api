using Application.Abstractions.Messaging;

namespace Application.Products.Variants.SearchAll;

public record SearchAllProductVariantsQuery(Guid ProductId) : IQuery<IEnumerable<ProductVariantResponse>>;
