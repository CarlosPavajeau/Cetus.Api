using Application.Abstractions.Messaging;

namespace Application.Products.Options.SearchAll;

public sealed record SearchAllProductOptionsQuery(Guid ProductId) : IQuery<IEnumerable<ProductOptionResponse>>;
