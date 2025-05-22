using Application.Abstractions.Messaging;

namespace Application.Products.Delete;

public sealed record DeleteProductCommand(Guid Id) : ICommand<bool>;
