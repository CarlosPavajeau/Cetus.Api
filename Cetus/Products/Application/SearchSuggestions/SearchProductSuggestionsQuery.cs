using Cetus.Products.Application.SearchForSale;
using MediatR;

namespace Cetus.Products.Application.SearchSuggestions;

public sealed record SearchProductSuggestionsQuery(Guid ProductId, Guid CategoryId) : IRequest<IEnumerable<ProductResponse>>;
