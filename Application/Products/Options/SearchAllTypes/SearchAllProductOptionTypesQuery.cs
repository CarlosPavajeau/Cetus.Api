using Application.Abstractions.Messaging;

namespace Application.Products.Options.SearchAllTypes;

public sealed record SearchAllProductOptionTypesQuery : IQuery<IEnumerable<ProductOptionTypeResponse>>;
