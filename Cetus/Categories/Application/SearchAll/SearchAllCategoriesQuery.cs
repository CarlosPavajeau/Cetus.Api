using MediatR;

namespace Cetus.Categories.Application.SearchAll;

public sealed record SearchAllCategoriesQuery : IRequest<IEnumerable<CategoryResponse>>;
