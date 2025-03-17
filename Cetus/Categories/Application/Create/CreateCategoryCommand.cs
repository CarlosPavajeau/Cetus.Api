using MediatR;

namespace Cetus.Categories.Application.Create;

public sealed record CreateCategoryCommand(string Name) : IRequest<bool>;
