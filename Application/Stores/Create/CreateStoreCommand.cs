using Application.Abstractions.Messaging;

namespace Application.Stores.Create;

public sealed record CreateStoreCommand(string Name, string Slug, string ExternalId) : ICommand;
