using Application.Abstractions.Messaging;

namespace Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name) : ICommand<bool>;
