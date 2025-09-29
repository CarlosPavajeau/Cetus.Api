using Application.Abstractions.Messaging;

namespace Application.Categories.FindBySlug;

public sealed record FindCategoryBySlugQuery(string Slug) : IQuery<FindCategoryBySlugResponse>;
