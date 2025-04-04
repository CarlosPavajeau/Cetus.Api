using MediatR;

namespace Cetus.Categories.Application.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : IRequest<bool>;
