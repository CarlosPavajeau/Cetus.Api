using Application.Abstractions.Messaging;
using Application.Products.Find;

namespace Application.Products.SearchForSale;

public sealed record SearchAllProductsForSaleQuery : IQuery<IEnumerable<ProductResponse>>;
