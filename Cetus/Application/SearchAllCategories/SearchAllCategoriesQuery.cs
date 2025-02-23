using MediatR;

namespace Cetus.Application.SearchAllCategories;

public sealed record
    SearchAllCategoriesQuery : IRequest<IEnumerable<CategoryResponse>>;
