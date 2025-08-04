using Application.Abstractions.Messaging;

namespace Application.Products.SearchSuggestions;

public sealed record SearchProductSuggestionsQuery(Guid ProductId) : IQuery<IEnumerable<SimpleProductForSaleResponse>>;
