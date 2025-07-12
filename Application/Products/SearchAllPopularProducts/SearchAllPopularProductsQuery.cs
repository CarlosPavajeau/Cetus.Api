using Application.Abstractions.Messaging;
using Application.Products.Find;

namespace Application.Products.SearchAllPopularProducts;

public record SearchAllPopularProductsQuery : IQuery<IEnumerable<ProductResponse>>;
