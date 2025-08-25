using Application.Abstractions.Messaging;

namespace Application.Products.Options.Create;

public sealed record CreateProductOptionCommand(Guid ProductId, long OptionTypeId) : ICommand;
