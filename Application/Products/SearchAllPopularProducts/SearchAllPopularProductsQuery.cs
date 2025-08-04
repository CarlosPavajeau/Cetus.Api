using Application.Abstractions.Messaging;

namespace Application.Products.SearchAllPopularProducts;

public record SearchAllPopularProductsQuery : IQuery<IEnumerable<SimpleProductForSaleResponse>>;
