using Application.Abstractions.Messaging;

namespace Application.Products.Find;

public sealed record FindProductQuery(Guid Id) : IQuery<ProductResponse?>;
