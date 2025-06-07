using Application.Abstractions.Messaging;

namespace Application.Products.TopSelling;

public sealed record GetTopSellingProductsQuery : IQuery<IEnumerable<TopSellingProductResponse>>;
