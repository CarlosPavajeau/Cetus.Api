using Application.Abstractions.Messaging;

namespace Application.Categories.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name) : ICommand<bool>;
