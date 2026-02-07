using Application.Abstractions.Messaging;

namespace Application.Stores.FindByDomain;

public sealed record FindStoreByDomainQuery(string Domain) : IQuery<SimpleStoreResponse>;
