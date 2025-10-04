using Application.Abstractions.Messaging;

namespace Application.Products.Find;

public sealed record FindProductBySlugQuery(string Slug) : IQuery<ProductForSaleResponse>;
