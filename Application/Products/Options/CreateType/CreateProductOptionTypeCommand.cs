using Application.Abstractions.Messaging;

namespace Application.Products.Options.CreateType;

public sealed record CreateProductOptionTypeCommand(string Name, IReadOnlyList<string> Values) : ICommand;
