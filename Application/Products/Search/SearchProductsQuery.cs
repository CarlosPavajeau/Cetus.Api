using Application.Abstractions.Messaging;

namespace Application.Products.Search;

public sealed record SearchProductsQuery(string SearchTerm) : IQuery<IEnumerable<SearchProductResponse>>;
