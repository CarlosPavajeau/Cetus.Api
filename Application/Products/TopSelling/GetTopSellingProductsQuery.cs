using Application.Abstractions.Messaging;
using Application.Products.SearchForSale;

namespace Application.Products.TopSelling;

public sealed record GetTopSellingProductsQuery : IQuery<IEnumerable<ProductResponse>>;
