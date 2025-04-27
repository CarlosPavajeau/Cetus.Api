using MediatR;

namespace Cetus.Products.Application.Find;

public sealed record FindProductQuery(Guid Id) : IRequest<ProductResponse?>;
