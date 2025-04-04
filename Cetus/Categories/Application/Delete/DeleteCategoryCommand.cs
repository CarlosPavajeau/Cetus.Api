using MediatR;

namespace Cetus.Categories.Application.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<bool>;
