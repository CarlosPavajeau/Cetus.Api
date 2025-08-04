using Application.Abstractions.Messaging;

namespace Application.Products.SearchForSale;

public sealed record SearchAllProductsForSaleQuery : IQuery<IEnumerable<SimpleProductForSaleResponse>>;
