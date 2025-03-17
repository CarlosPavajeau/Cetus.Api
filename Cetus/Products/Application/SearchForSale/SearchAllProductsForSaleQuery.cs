using MediatR;

namespace Cetus.Products.Application.SearchForSale;

public sealed record SearchAllProductsForSaleQuery : IRequest<IEnumerable<ProductResponse>>;
