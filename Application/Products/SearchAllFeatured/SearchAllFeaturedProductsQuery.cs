using Application.Abstractions.Messaging;

namespace Application.Products.SearchAllFeatured;

public record SearchAllFeaturedProductsQuery : IQuery<IEnumerable<SimpleProductForSaleResponse>>;
