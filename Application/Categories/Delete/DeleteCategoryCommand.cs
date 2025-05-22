using Application.Abstractions.Messaging;

namespace Application.Categories.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand<bool>;
