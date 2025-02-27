using MediatR;

namespace Cetus.Application.SearchAllProductsForSale;

public sealed record SearchAllProductsForSaleQuery : IRequest<IEnumerable<ProductResponse>>;
