using Cetus.Application.SearchAllProducts;
using MediatR;

namespace Cetus.Application.FindProduct;

public sealed record FindProductQuery(Guid Id) : IRequest<ProductResponse?>;
