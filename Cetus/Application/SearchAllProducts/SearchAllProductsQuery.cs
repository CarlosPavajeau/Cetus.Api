using MediatR;

namespace Cetus.Application.SearchAllProducts;

public sealed record
    SearchAllProductsQuery : IRequest<IEnumerable<ProductResponse>>;
