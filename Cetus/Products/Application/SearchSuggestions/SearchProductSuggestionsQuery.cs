using Cetus.Products.Application.SearchForSale;
using MediatR;

namespace Cetus.Products.Application.SearchSuggestions;

public sealed record SearchProductSuggestionsQuery(Guid ProductId) : IRequest<IEnumerable<ProductResponse>>;
