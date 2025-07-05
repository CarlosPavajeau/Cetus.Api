using Application.Abstractions.Messaging;
using Application.Categories.SearchAll;

namespace Application.Categories.Create;

public sealed record CreateCategoryCommand(string Name) : ICommand<CategoryResponse>;
