using Application.Abstractions.Messaging;
using Application.Categories.SearchAll;

namespace Application.Categories.FindBySlug;

public sealed record FindCategoryBySlugQuery(string Slug) : IQuery<FindCategoryBySlugResponse>;
