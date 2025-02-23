using MediatR;

namespace Cetus.Application.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : IRequest<bool>;
