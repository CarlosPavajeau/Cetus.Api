using MediatR;

namespace Cetus.Products.Application.SearchAll;

public sealed record SearchAllProductsQuery : IRequest<IEnumerable<ProductResponse>>;
