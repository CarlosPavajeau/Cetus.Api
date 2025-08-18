using Application.Abstractions.Messaging;

namespace Application.Products.Options.CreateType;

public sealed record CreateProductOptionTypeCommand(string Name, string[] Values) : ICommand;
