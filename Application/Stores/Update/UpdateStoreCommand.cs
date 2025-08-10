using Application.Abstractions.Messaging;

namespace Application.Stores.Update;

public sealed record UpdateStoreCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Address,
    string? Phone,
    string? Email,
    string? CustomDomain) : ICommand<StoreResponse>;
