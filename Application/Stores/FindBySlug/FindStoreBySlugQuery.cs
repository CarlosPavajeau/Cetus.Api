using Application.Abstractions.Messaging;

namespace Application.Stores.FindBySlug;

public sealed record FindStoreBySlugQuery(string Slug) : IQuery<SimpleStoreResponse>;
