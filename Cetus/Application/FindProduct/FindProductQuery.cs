using Cetus.Application.SearchAllProductsForSale;
using MediatR;

namespace Cetus.Application.FindProduct;

public sealed record FindProductQuery(Guid Id) : IRequest<ProductResponse?>;
