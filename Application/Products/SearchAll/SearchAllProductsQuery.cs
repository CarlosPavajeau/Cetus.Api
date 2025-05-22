using Application.Abstractions.Messaging;

namespace Application.Products.SearchAll;

public sealed record SearchAllProductsQuery : IQuery<IEnumerable<ProductResponse>>;
