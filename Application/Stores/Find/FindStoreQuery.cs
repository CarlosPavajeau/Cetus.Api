using Application.Abstractions.Messaging;

namespace Application.Stores.Find;

public sealed record FindStoreQuery(string? CustomDomain, string? Slug) : IQuery<SimpleStoreResponse>;
