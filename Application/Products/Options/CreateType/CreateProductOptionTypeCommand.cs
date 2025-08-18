using Application.Abstractions.Messaging;

namespace Application.Products.Options.CreateType;

public record CreateProductOptionTypeCommand(string Name, string[] Values) : ICommand;
