using Application.Abstractions.Messaging;

namespace Application.Products.SearchAllByCategory;

public sealed record SearchAllProductsByCategory(Guid CategoryId) : IQuery<IEnumerable<SimpleProductForSaleResponse>>;
