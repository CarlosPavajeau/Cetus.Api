using Application.Abstractions.Messaging;

namespace Application.Categories.SearchAll;

public sealed record SearchAllCategoriesQuery : IQuery<IEnumerable<CategoryResponse>>;
