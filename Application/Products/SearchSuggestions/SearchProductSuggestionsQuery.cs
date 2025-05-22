using Application.Abstractions.Messaging;
using Application.Products.SearchForSale;

namespace Application.Products.SearchSuggestions;

public sealed record SearchProductSuggestionsQuery(Guid ProductId) : IQuery<IEnumerable<ProductResponse>>;
