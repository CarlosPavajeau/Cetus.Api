using Application.Abstractions.Messaging;

namespace Application.Stores.FindByExternalId;

public sealed record FindStoreByExternalId(string ExternalId) : IQuery<StoreResponse>;
