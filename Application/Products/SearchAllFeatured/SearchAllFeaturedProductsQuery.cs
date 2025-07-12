using Application.Abstractions.Messaging;
using Application.Products.Find;

namespace Application.Products.SearchAllFeatured;

public record SearchAllFeaturedProductsQuery : IQuery<IEnumerable<ProductResponse>>;
